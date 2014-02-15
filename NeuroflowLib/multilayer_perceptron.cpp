#include "stdafx.h"
#include "multilayer_perceptron.h"
#include "prop_def.h"
#include "layer_order_comparer.h"
#include "layer_connections.h"
#include "supervised_learning_behavior.h"
#include "layer.h"
#include "device_array_management.h"
#include "computation_context.h"
#include "device_array.h"
#include "device_array2.h"
#include "data_array.h"
#include "activation_description.h"

USING

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const optional_properties_t& properties) :
    contexted(context),
    _computeActivation(context->compute_activation()),
    _daMan(context->device_array_management()),
    _gradientsPool(_daMan->create_pool(false)),
    _gradientSumsPool(_daMan->create_pool(false)),
    _outputs(_daMan->create_pool(false)),
    _netValueDerivates(_daMan->create_pool(false)),
    _biases(_daMan->create_pool(false)),
    _errors(_daMan->create_pool(false)),
    _weights(_daMan->create_pool(true)),
    _biasGradients(_gradientsPool),
    _biasGradientSums(_gradientSumsPool),
    _gradients(_gradientsPool),
    _gradientSums(_gradientSumsPool)
{
    prop_def pd(_properties, properties);
    _gradientComputationMethod = pd.defEnum(prop_gradient_computation_method, gradient_computation_method::feed_forward);
    _maxBpttIterations = pd.def<idx_t>(prop_max_bptt_iterations, idx_t(0), [](idx_t v) { return v >= 0; });

    _layers = layers | sort(layer_order_comparer()) | row_num() | to_vector();

    // It supposed to be cool, but looks like shit because of MSVC.    
    auto infos = _layers
    | select([](row_numbered<layer_ptr>& l) -> pair<idx_t, supervised_learning_behavior_ptr> { return make_pair(l.row_num(), from(l.value()->behaviors()) | dcast<supervised_learning_behavior>() | first_or_default()); })
    | select([](pair<idx_t, supervised_learning_behavior_ptr>& r)
    { 
        return layer_info(
            r.first, 
            r.second != null ? r.second->weight_update_mode() == weight_update_mode::online : false, // is_online
            r.second != null ? r.second->weight_update_mode() == weight_update_mode::offline : false, // is_offline
            r.second != null ? r.second->optimization_type() : learning_algo_optimization_type::none);
    })
    | to_map([](layer_info& i) { return i.index; });

    auto infoValues = infos | select([](pair<const idx_t, layer_info>& p) { return p.second; });

    _isTrainingEnabled = infos.size() > 0;

    _isGradientsCalculated = infoValues | any([](layer_info& i) { return i.optimization_type == learning_algo_optimization_type::gradient_based; });

    _calculateGlobalOfflineError = infoValues | any([](layer_info& i) { return i.optimization_type == learning_algo_optimization_type::global && i.is_offline; });
    _calculateGlobalOnlineError = _calculateGlobalOfflineError || infoValues | any([](layer_info& i) { return i.optimization_type == learning_algo_optimization_type::global && i.is_online; });

    _doBackpropagate = _isTrainingEnabled && _isGradientsCalculated && (_gradientComputationMethod == gradient_computation_method::feed_forward || _gradientComputationMethod == gradient_computation_method::bptt);

    _isRecurrent = _layers | any([](row_numbered<layer_ptr>& l) { return l.value()->has_recurrent_connections(); });

    _doFFBP = _doBackpropagate && !_isRecurrent && _gradientComputationMethod == gradient_computation_method::feed_forward;
    _doBPTT = _doBackpropagate && _isRecurrent && _gradientComputationMethod == gradient_computation_method::bptt;
    _doRTLR = _isTrainingEnabled && _isRecurrent && _gradientComputationMethod == gradient_computation_method::rtlr;

    if (_isRecurrent && _isGradientsCalculated && !(_doBPTT || _doRTLR)) throw_logic_error("Recurrent Multilayer Perceptron cannot be trained by Feed Forward gradient computation algorithms.");

    if (_doBPTT)
    {
        if (_maxBpttIterations <= 0) throw_invalid_argument("Max BPTT iterations must be greater than zero.");
    }
    else
    {
        _maxBpttIterations = 0;
    }

    create_structure(infos);
    
    /*create_compute();
    create_train_init();
    create_train(infos);*/
}

const boost::property_tree::ptree& multilayer_perceptron::properties() const
{
    return _properties;
}

nf::gradient_computation_method multilayer_perceptron::gradient_computation_method() const
{
    return _gradientComputationMethod;
}

idx_t multilayer_perceptron::max_bptt_iterations() const
{
    return _maxBpttIterations;
}

idx_t multilayer_perceptron::input_size() const
{
    return _layers.front().value()->size();
}

idx_t multilayer_perceptron::output_size() const
{
    return _layers.back().value()->size();
}

idx_t multilayer_perceptron::number_of_weights() const
{
    return _weights.size() + _biases.size();
}

void multilayer_perceptron::create_structure(std::map<idx_t, layer_info>& infos)
{
    for (idx_t lidx = 0; lidx < _layers.size(); lidx++)
    {
        auto& learningInfo = infos.find(lidx)->second;
        bool isInput = lidx == 0;
        bool isOutput = lidx == _layers.size() - 1;

        auto& layer = _layers[lidx];
        idx_t layerSize = layer.value()->size();

        if (isInput)
        {
            if (_doBPTT) _bpttNetInputs = _daMan->create_array(false, input_size() * _maxBpttIterations);
        }
        else
        {
            // Output:
            if (!isOutput)
            {
                _outputs.add(lidx, _doBPTT ? layerSize * _maxBpttIterations : layerSize);
            }

            // Net Value Derivates:
            if (_doRTLR)
            {
                _netValueDerivates.add(lidx, layerSize);
            }

            // Bias:
            _biases.add(lidx, layerSize);

            // For gradients:
            if (_doBackpropagate)
            {
                // Errors:
                _errors.add(lidx, layerSize);
            }

            if (learningInfo.is_offline || _doBPTT)
            {
                // Bias Gradients:
                _biasGradients.add(lidx, layerSize);
            }

            if (learningInfo.is_offline)
            {
                // Bias Gradient Sums:
                _biasGradientSums.add(lidx, layerSize);
            }

            idx_t inputLayersSize = layer.value()->input_layers() | size();
            for (idx_t iidx = 0; iidx < inputLayersSize; iidx++)
            {
                auto inputLayer = layer.value()->get_input_layer(iidx);
                auto key = make_pair(get_layer_index(inputLayer), lidx);

                // Weights
                _weights.add(key, inputLayer->size(), layer.value()->size());

                if (learningInfo.is_online || _doBPTT)
                {
                    // Gradients:
                    _gradients.add(key, inputLayer->size(), layer.value()->size());
                }

                if (learningInfo.is_offline)
                {
                    // Gradient Sums:
                    _gradientSums.add(key, inputLayer->size(), layer.value()->size());
                }
            }
        }
    }
}

idx_t multilayer_perceptron::get_layer_index(const layer_ptr& layer)
{
    return _layers
        | where([=](row_numbered<layer_ptr>& l) { return l.value() == layer; })
        | select([](row_numbered<layer_ptr>& l) { return l.row_num(); })
        | first();
}

void multilayer_perceptron::get_weights(const data_array_ptr& to) const
{
    verify_arg(to != null, "Argument 'to' is null.");

    idx_t sIdx = 0;

    for (auto& bias : _biases.get_arrays())
    {
        _daMan->copy(bias, 0, to, sIdx, bias->size());
        sIdx += bias->size();
    }

    for (auto& weight : _weights.get_arrays())
    {
        _daMan->copy(weight, 0, to, sIdx, weight->size());
        sIdx += weight->size();
    }
}

void multilayer_perceptron::set_weights(const data_array_ptr& from)
{
    verify_arg(from != null, "Argument 'from' is null.");

    idx_t sIdx = 0;

    for (auto& bias : _biases.get_arrays())
    {
        _daMan->copy(from, sIdx, bias, 0, bias->size());
        sIdx += bias->size();
    }

    for (auto& weight : _weights.get_arrays())
    {
        _daMan->copy(from, sIdx, weight, 0, weight->size());
        sIdx += weight->size();
    }
}

activation_description multilayer_perceptron::get_activation_desc(idx_t layerIndex)
{
    auto& layer = _layers[layerIndex];
    auto desc = layer.value()->descriptions() | dcast<activation_description>() | first_or_default();
    if (!desc) throw_runtime_error("Layer " + to_string(layer.row_num()) + " activation description expected.");
    return *desc;
}