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
#include "mlp_forward_node.h"
#include "mlp_backward_node.h"
#include "compute_activation.h"

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
    create_compute();
    create_train(infos);
    
    /*
    create_train_init();
    */
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
    for (idx_t lidx = 1; lidx < _layers.size(); lidx++)
    {
        auto& learningInfo = infos.find(lidx)->second;
        bool isOutput = lidx == _layers.size() - 1;

        auto& layer = _layers[lidx];
        idx_t layerSize = layer.value()->size();

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

void multilayer_perceptron::create_compute()
{
    vector<mlp_forward_node> nodes(_layers.size() - 1);
    for (idx_t lidx = 1; lidx < _layers.size(); lidx++)
    {
        auto& layer = _layers[lidx];
        auto& node = nodes[lidx - 1];
        
        for (auto& inputConnectedLayer : layer.value()->input_layers())
        {
            idx_t inputIndex = get_layer_index(inputConnectedLayer);
            auto key = make_pair(inputIndex, lidx);
            node.in.emplace_back([=](){ return get_net_values(inputIndex); }, _weights.get(key));
        }

        node.activation = get_activation_desc(lidx);
        node.bias = _biases.get(lidx);
        node.out = [=](){ return get_net_values(lidx); };
        if (_doRTLR) node.derivate = _netValueDerivates.get(lidx);
    }

    if (_doRTLR)
    {
        throw_logic_error("RTLR is not implemented.");
    }
    else if (_doBPTT)
    {
        throw_logic_error("BPTT is not implemented.");
    }
    else
    {
        computeFunc = bind([=](const nf_object_ptr& ctx, const vector<mlp_forward_node>& nodes, idx_t offset)
        {
            _computeActivation->compute_forward(ctx, nodes, offset);
        },
        move(_computeActivation->create_operation_context()),
        move(nodes),
        _1);
    }
}

void multilayer_perceptron::create_train(std::map<idx_t, layer_info>& infos)
{
    if (_doBackpropagate)
    {
        /*
        activation_description activation;
        std::vector<get_device_array_ptr_t> in;
        std::vector<device_array2_ptr> gradients;
        std::vector<device_array2_ptr> gradient_sums;
        get_device_array_ptr_t out;
        device_array_ptr bias_gradients;
        device_array_ptr bias_gradient_sums;
        std::vector<weighted_errors> lower_errors;
        */
        vector<mlp_backward_node> nodes(_layers.size() - 1);
        for (idx_t lidx = _layers.size() - 1, nodeidx = 0; lidx >= 1; lidx--, nodeidx++)
        {
            auto& layer = _layers[lidx];
            auto& node = nodes[nodeidx];
            auto& learningInfo = infos.find(lidx)->second;
            if (!learningInfo.is_offline && !learningInfo.is_online) continue;

            for (auto& inputConnectedLayer : layer.value()->input_layers())
            {
                idx_t inputIndex = get_layer_index(inputConnectedLayer);
                auto key = make_pair(inputIndex, lidx);
                if (_doFFBP)
                {
                    node.in.push_back([=]() { return get_net_values(inputIndex); });
                }
                else if (_doBPTT)
                {
                    node.in.push_back([=]()
                    {
                        throw_runtime_error("Not implemented! Stack based input copy needed, see MultiplayerPerceptron.cs!");
                    });
                }
                if (learningInfo.is_online || _doBPTT) node.gradients.push_back(_gradients.get(key));
                if (learningInfo.is_offline) node.gradient_sums.push_back(_gradientSums.get(key));
            }

            if (nodeidx == 0)
            {
                // Last layer
                node.net_outputs = supervised_outputs([=](){ return get_net_values(lidx); }, get_net_desired_outputs);
            }
            else
            {
                for (auto& outputConnectedLayer : layer.value()->output_layers())
                {
                    idx_t outputIndex = get_layer_index(outputConnectedLayer);
                    auto key = make_pair(lidx, outputIndex);
                    node.lower_errors.emplace_back(_errors.get(outputIndex), _weights.get(key));
                }
            }

            node.activation = get_activation_desc(lidx);

            if (learningInfo.is_online || _doBPTT) node.bias_gradients = _biasGradients.get(lidx);
            if (learningInfo.is_offline) node.bias_gradient_sums = _biasGradientSums.get(lidx);
        }

        trainFunc = bind([=](const nf_object_ptr& ctx, const vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf)
        {
            _computeActivation->compute_backward(ctx, nodes, offset, gcf);
        },
        move(_computeActivation->create_operation_context()),
        move(nodes),
        _1,
        _2);
    }
    else if (_doRTLR)
    {
        _rtlr.initialize(this);
    }
    if (_calculateGlobalOnlineError || _calculateGlobalOfflineError)
    {
        throw_runtime_error("Not implemented!");
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

const device_array_ptr& multilayer_perceptron::get_net_values(idx_t layerIndex) const
{
    if (layerIndex == 0)
    {
        return _netInputs;
    }
    else if (layerIndex == _layers.size() - 1)
    {
        return _netOutputs;
    }
    else
    {
        return _outputs.get(layerIndex); 
    }
}

const device_array_ptr& multilayer_perceptron::get_net_desired_outputs(idx_t layerIndex) const
{
    return _netDesiredOutputs;
}

void multilayer_perceptron::compute(const data_array_ptr& inputs, const data_array_ptr& outputs)
{
    verify_arg(inputs != null, "Argument 'inputs' is null.");
    verify_arg(outputs != null, "Argument 'outputs' is null.");

    compute_sample_entry(inputs, outputs);
}

void multilayer_perceptron::compute(const data_array_collection_t& inputs, const data_array_collection_t& outputs)
{
    verify_arg(!inputs.empty(), "Argument 'inputs' is empty.");
    verify_arg(!outputs.empty(), "Argument 'outputs' is empty.");
    verify_arg(inputs.size() == outputs.size(), "Argument collections sizes are not match.");

    idx_t size = inputs.size();
    for (idx_t i = 0; i < size; i++) compute_sample_entry(inputs[i], outputs[i]);
}

void multilayer_perceptron::compute_sample_entry(const device_array_ptr& inputs, const device_array_ptr& outputs)
{
    setup_net_values(inputs, outputs);
    computeFunc((idx_t)0);
}

void multilayer_perceptron::setup_net_values(const device_array_ptr& inputs, const device_array_ptr& outputs)
{
    _netInputs = inputs;
    _netOutputs = outputs;
}