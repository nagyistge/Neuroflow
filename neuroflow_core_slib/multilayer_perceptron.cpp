#include "stdafx.h"
#include "multilayer_perceptron.h"
#include "layer_order_comparer.h"
#include "layer_connections.h"
#include "supervised_learning_behavior.h"
#include "learning_init_behavior.h"
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
#include "equatable_ptr.h"
#include "training_node.h"
#include "supervised_learning.h"
#include "initialize_learning.h"
#include "learning_impl_factory.h"
#include "mlp_init_pars.h"
#include "supervised_batch.h"
#include "utils.h"
#include "device_array_pool.h"

USING
namespace ph = std::placeholders;

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, layer_collection_t& layers, const mlp_init_pars* properties) :
    contexted(context),
    _computeActivation(context->compute_activation()),
    _daMan(context->device_array_management()),
    _learningImplFactory(context->learning_impl_factory()),
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
    assert(context);
    assert(properties);
    assert(_daMan);
    assert(_computeActivation);
    assert(_learningImplFactory);

    _gradientComputationMethod = properties->gradient_computation_method;
    _maxBpttIterations = properties->max_bptt_iterations;

    //Create ordered layers:

    // Copy layers
    auto copyOfLayers = from(layers) >> to_vector();
    // Sort
    layer_order_comparer comparer;
    sort(copyOfLayers.begin(), copyOfLayers.end(), [&](const layer_ptr& l1, const layer_ptr& l2) { return comparer(l1, l2) < 0; });
    // Project:
    idx_t idx = 0;
    _layers = from(copyOfLayers) >> select([&](const layer_ptr& l) { return row_numbered<layer_ptr>(idx++, l); }) >> to_vector();

    auto infos = from(_layers) >>
        select([](const row_numbered<layer_ptr>& l) -> pair<idx_t, supervised_learning_behavior_ptr>
        {
            return make_pair(
                        l.row_num(),
                        from(l.value()->behaviors()) >>
                        select([](const layer_behavior_ptr& b) { return dynamic_pointer_cast<supervised_learning_behavior>(b); }) >>
                        where([](const supervised_learning_behavior_ptr& b) { return b != null; }) >>
                        first_or_default());
        }) >>
        select([](const pair<idx_t, supervised_learning_behavior_ptr>& r)
        {
            return layer_info(
                r.first,
                r.second != null ? r.second->weight_update_mode() == weight_update_mode::online : false, // is_online
                r.second != null ? r.second->weight_update_mode() == weight_update_mode::offline : false, // is_offline
                r.second != null ? r.second->optimization_type() : learning_algo_optimization_type::none);
        }) >>
        to_map([](const layer_info& i) { return i.index; });

    auto infoValues = from(infos) >> select([](const pair<const idx_t, layer_info>& p) { return p.second; }) >> to_list();

    _isTrainingEnabled = infos.size() > 0;

    _isGradientsCalculated = from(infoValues) >> any([](const layer_info& i) { return i.optimization_type == learning_algo_optimization_type::gradient_based; });

    _calculateGlobalOfflineError = from(infoValues) >> any([](const layer_info& i) { return i.optimization_type == learning_algo_optimization_type::global && i.is_offline; });
    _calculateGlobalOnlineError = _calculateGlobalOfflineError || (from(infoValues) >> any([](const layer_info& i) { return i.optimization_type == learning_algo_optimization_type::global && i.is_online; }));

    _doBackpropagate = _isTrainingEnabled && _isGradientsCalculated && (_gradientComputationMethod == nf::gradient_computation_method::feed_forward || _gradientComputationMethod == nf::gradient_computation_method::bptt);

    _isRecurrent = from(_layers) >> any([](const row_numbered<layer_ptr>& l) { return l.value()->has_recurrent_connections(); });

    _doFFBP = _doBackpropagate && !_isRecurrent && _gradientComputationMethod == nf::gradient_computation_method::feed_forward;
    _doBPTT = _doBackpropagate && _isRecurrent && _gradientComputationMethod == nf::gradient_computation_method::bptt;
    _doRTLR = _isTrainingEnabled && _isRecurrent && _gradientComputationMethod == nf::gradient_computation_method::rtlr;

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
    create_training(infos);
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
    if (_doBPTT)
    {
        _bpttNetInputs = _daMan->create_array(false, input_size() * _maxBpttIterations);
        _netInputs = _bpttNetInputs.get();
    }

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
        else if (_doBPTT)
        {
            _bpttNetOutputs = _daMan->create_array(false, layerSize * _maxBpttIterations);
            _bpttNetDesiredOutputs = _daMan->create_array(false, layerSize * _maxBpttIterations);
            _netOutputs = _bpttNetInputs.get();
            _netDesiredOutputs = _bpttNetDesiredOutputs.get();
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

        if (learningInfo.is_online || _doBPTT)
        {
            // Bias Gradients:
            _biasGradients.add(lidx, layerSize);
        }

        if (learningInfo.is_offline)
        {
            // Bias Gradient Sums:
            _biasGradientSums.add(lidx, layerSize);
        }

        for (auto& inputConnectedLayer : layer.value()->input_layers())
        {
            auto key = make_pair(get_layer_index(inputConnectedLayer), lidx);

            // Weights
            _weights.add(key, inputConnectedLayer->size(), layer.value()->size());

            if (learningInfo.is_online || _doBPTT)
            {
                // Gradients:
                _gradients.add(key, inputConnectedLayer->size(), layer.value()->size());
            }

            if (learningInfo.is_offline)
            {
                // Gradient Sums:
                _gradientSums.add(key, inputConnectedLayer->size(), layer.value()->size());
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
        bool isLast = lidx == _layers.size() - 1;
        
        for (auto& inputConnectedLayer : layer.value()->input_layers())
        {
            idx_t inputIndex = get_layer_index(inputConnectedLayer);
            auto key = make_pair(inputIndex, lidx);
            node.in.emplace_back([=](){ return get_net_values(inputIndex); }, _weights.get(key));
        }

        node.activation = get_activation_desc(lidx);
        node.bias = _biases.get(lidx);
        node.out = [=](){ return get_net_values(lidx); };

        if (_doRTLR)
        {
            node.derivates = _netValueDerivates.get(lidx);
        }
    }

    if (_doRTLR)
    {
        _computeFunc = std::bind([=](const nf_object_ptr& ctx, const vector<mlp_forward_node>& nodes, idx_t bpttIterationsCount)
        {
            _computeActivation->compute_forward(ctx, nodes, bpttIterationsCount);
            _rtlr->compute_gradients(_netDesiredOutputs);
        },
        move(_computeActivation->create_operation_context()),
        move(nodes),
        ph::_1);
    }
    else 
    {
        _computeFunc = std::bind([=](const nf_object_ptr& ctx, const vector<mlp_forward_node>& nodes, idx_t bpttIterationsCount)
        {
            _computeActivation->compute_forward(ctx, nodes, bpttIterationsCount);
        },
        move(_computeActivation->create_operation_context()),
        move(nodes),
        ph::_1);
    }
}

void multilayer_perceptron::create_training(std::map<idx_t, layer_info>& infos)
{
    if (_doBackpropagate)
    {
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
                        return get_net_values(inputIndex);
                    });
                }
                if (learningInfo.is_online || _doBPTT) node.gradients.push_back(_gradients.get(key));
                if (learningInfo.is_offline) node.gradient_sums.push_back(_gradientSums.get(key));
            }

            if (nodeidx == 0)
            {
                // Last layer
                node.net_outputs = supervised_outputs([=]()
                { 
                    return get_net_values(lidx); 
                }, [=]() 
                { 
                    return get_net_desired_outputs(); 
                });
            }
            else
            {
                for (auto& outputConnectedLayer : layer.value()->output_layers())
                {
                    idx_t outputIndex = get_layer_index(outputConnectedLayer);
                    auto key = make_pair(lidx, outputIndex);
                    node.lower_errors.emplace_back(_errors.get(outputIndex), _weights.get(key));
                }
                node.out = [=](){ return get_net_values(lidx); };
            }

            node.activation = get_activation_desc(lidx);
            node.errors = _errors.get(lidx);            
            if (learningInfo.is_online || _doBPTT) node.bias_gradients = _biasGradients.get(lidx);
            if (learningInfo.is_offline) node.bias_gradient_sums = _biasGradientSums.get(lidx);
        }

        _trainFunc = std::bind([=](const nf_object_ptr& ctx, const vector<mlp_backward_node>& nodes, idx_t bpttIterationsCount, gradient_computation_formula gcf, idx_t inItIdx)
        {
            _computeActivation->compute_backward(ctx, nodes, bpttIterationsCount, gcf, inItIdx);
        },
        move(_computeActivation->create_operation_context()),
        move(nodes),
        ph::_1,
        ph::_2,
        ph::_3);
    }
    else if (_doRTLR)
    {
        _rtlr = move(rtlr());
        _rtlr->initialize(this);
    }
    if (_calculateGlobalOnlineError || _calculateGlobalOfflineError)
    {
        throw_not_implemented();
    }
    create_impls();
}

template<typename TB>
std::unordered_map<equatable_ptr<TB>, std::set<idx_t>> multilayer_perceptron::collect_layer_indexes()
{
    typedef shared_ptr<TB> PTB;

    unordered_map<equatable_ptr<TB>, set<idx_t>> layerIndexes;

    for (auto& layer : _layers)
    {
        for (auto& behavior : layer.value()->behaviors())
        {
            auto wantedBehavior = dynamic_pointer_cast<TB>(behavior);
            if (wantedBehavior)
            {
                auto item = row_numbered<PTB>(layer.row_num(), wantedBehavior);
                auto key = make_equatable_ptr(item.value());
                auto it = layerIndexes.find(key);
                if (it != layerIndexes.end())
                {
                    //found:
                    it->second.insert(item.row_num());
                }
                else
                {
                    // not found:
                    layerIndexes.insert(make_pair(key, set<idx_t>({ item.row_num() })));
                }
            }
        }
    }

    return move(layerIndexes);
}

void multilayer_perceptron::create_impls()
{
    // Init Layers
    auto values = values_for_training_t();
    for (idx_t lidx = 1; lidx < _layers.size(); lidx++)
    {
        auto& layer = _layers[lidx];
        device_array_collection_t weights;
        boost::optional<device_array_collection_t> gradients;
        boost::optional<device_array_collection_t> gradientSums;

        weights.push_back(_biases.get(lidx));
        device_array_ptr arr;
        if (_biasGradients.try_get(lidx, arr))
        {
            if (!gradients) gradients = device_array_collection_t();
            gradients->push_back(arr);
        }
        if (_biasGradientSums.try_get(lidx, arr))
        {
            if (!gradientSums) gradientSums = device_array_collection_t();
            gradientSums->push_back(arr);
        }

        for (auto& inputConnectedLayer : layer.value()->input_layers())
        {
            idx_t inputIndex = get_layer_index(inputConnectedLayer);
            auto key = make_pair(inputIndex, lidx);
            weights.push_back(_weights.get(key));
            device_array2_ptr arr2;
            if (_gradients.try_get(key, arr2)) 
            {
                gradients->push_back(arr2);
            }
            if (_gradientSums.try_get(key, arr2))
            {
                gradientSums->push_back(arr2);
            }
        }

        if (gradients) assert(gradients->size() == weights.size());
        if (gradientSums) assert(gradientSums->size() == weights.size());

        values.emplace_back(std::move(weights), std::move(gradients), std::move(gradientSums));
    }

    vector<learning_impl_ptr> initImpls;
    vector<supervised_learning_ptr> supervisedImpls;
    vector<supervised_learning_ptr> onlineImpls;
    vector<supervised_learning_ptr> offlineImpls;

    for (auto& il : collect_layer_indexes<learning_init_behavior>())
    {
        initImpls.push_back(create_learning_impl<learning_impl>(il.first.ptr(), from(il.second) >> to_vector(), values));
    }

    for (auto& sl : collect_layer_indexes<supervised_learning_behavior>())
    {
        auto impl = create_learning_impl<supervised_learning>(sl.first.ptr(), from(sl.second) >> to_vector(), values);
        if (idx_t(impl->iteration_type() & supervised_learning_iteration_type::online) != idx_t(0))
        {
            onlineImpls.push_back(impl);
        }
        if (idx_t(impl->iteration_type() & supervised_learning_iteration_type::offline) != idx_t(0))
        {
            offlineImpls.push_back(impl);
        }
        supervisedImpls.push_back(impl);
    }

    _initLearningFunc = std::bind([](const vector<learning_impl_ptr>& initImpls, const vector<supervised_learning_ptr>& supervisedImpls)
    {
        for (auto& impl : initImpls) impl->initialize();
        for (auto& impl : supervisedImpls) impl->initialize();
    },
    std::move(initImpls),
    std::move(supervisedImpls));

    _onlineLearningFunc = std::bind([](const vector<supervised_learning_ptr>& impls, const device_array_ptr& error)
    { 
        for (auto& impl : impls) impl->run(0, error);
    },
    std::move(onlineImpls),
    ph::_1);

    _offlineLearningFunc = std::bind([](const vector<supervised_learning_ptr>& impls, idx_t iterationCount, const device_array_ptr& error)
    {
        for (auto& impl : impls) impl->run(iterationCount, error);
    },
    std::move(offlineImpls),
    ph::_1,
    ph::_2);
}

template<typename I>
std::shared_ptr<I> multilayer_perceptron::create_learning_impl(const learning_behavior_ptr& behavior, const std::vector<idx_t>& forLayerIndexes, const values_for_training_t& values)
{
    auto layerNodes = make_shared<training_node_collection_t>();
    for (idx_t layerIndex : forLayerIndexes)
    {
        auto& currentValues = values[layerIndex - 1];
        for (idx_t arrayIndex = 0; arrayIndex < get<0>(currentValues).size(); arrayIndex++)
        {
            device_array_ptr weights, gradients, gradientSums;

            weights = get<0>(currentValues)[arrayIndex];
            if (get<1>(currentValues)) gradients = (*get<1>(currentValues))[arrayIndex];
            if (get<2>(currentValues)) gradientSums = (*get<2>(currentValues))[arrayIndex];
            layerNodes->emplace_back(weights, gradients, gradientSums);
        }
    }
    auto impl = dynamic_pointer_cast<I>(_learningImplFactory->create_impl(behavior, layerNodes));
    if (!impl)
    {
        auto e = string("Cannot create Learning Algorithm implementation for behavior: '") + typeid(*behavior).name() + "'.";
        throw_runtime_error(e);
    }
    return impl;
}

idx_t multilayer_perceptron::get_layer_index(const layer_ptr& layer)
{
    return from(_layers) >>
        where([=](const row_numbered<layer_ptr>& l) { return l.value() == layer; }) >>
        select([](const row_numbered<layer_ptr>& l) { return l.row_num(); }) >>
        first();
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
    auto desc = from(layer.value()->descriptions()) >>
            select([](const layer_description_ptr& d) { return dynamic_pointer_cast<activation_description>(d); }) >>
            where([](const layer_description_ptr& d) { return d != null; }) >>
            first_or_default();

    if (!desc)
    {
        throw_runtime_error("Layer " + to_string(layer.row_num()) + " activation description expected.");
    }
    return *desc;
}

device_array* multilayer_perceptron::get_net_values(idx_t layerIndex) const
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
        return _outputs.get(layerIndex).get(); 
    }
}

device_array* multilayer_perceptron::get_net_desired_outputs() const
{
    return _netDesiredOutputs;
}

void multilayer_perceptron::compute(const data_array_ptr& inputs, const data_array_ptr& outputs)
{
    verify_arg(inputs != null, "Argument 'inputs' is null.");
    verify_arg(outputs != null, "Argument 'outputs' is null.");

    compute_sample_entry(inputs, outputs, null);
}

void multilayer_perceptron::compute(const data_array_collection_t& inputs, const data_array_collection_t& outputs)
{
    verify_arg(!inputs.empty(), "Argument 'inputs' is empty.");
    verify_arg(!outputs.empty(), "Argument 'outputs' is empty.");
    verify_arg(inputs.size() == outputs.size(), "Argument collections sizes are not match.");

    idx_t size = inputs.size();
    for (idx_t i = 0; i < size; i++) compute_sample_entry(inputs[i], outputs[i], null);
}

void multilayer_perceptron::compute_sample_entry(const data_array_ptr& inputs, const data_array_ptr& outputs, const data_array_ptr& desiredOutputs, idx_t bpttIterationsCount)
{
    setup_net_values(inputs, outputs, desiredOutputs, bpttIterationsCount);
    _computeFunc(bpttIterationsCount);
}

void multilayer_perceptron::setup_net_values(const data_array_ptr& inputs, const data_array_ptr& outputs, const data_array_ptr& desiredOutputs, idx_t bpttIterationsCount)
{
    if (!_doBPTT)
    {
        _netInputs = inputs.get();
        _netOutputs = outputs.get();
        _netDesiredOutputs = desiredOutputs ? desiredOutputs.get() : null;
    }
    else
    {
        // If there is BPTT we should remember the values:
        _daMan->copy(inputs, 0, _bpttNetInputs, bpttIterationsCount * inputs->size(), inputs->size());
        _daMan->copy(outputs, 0, _bpttNetOutputs, bpttIterationsCount * outputs->size(), outputs->size());
        if (desiredOutputs)
        {
            _daMan->copy(desiredOutputs, 0, _bpttNetDesiredOutputs, bpttIterationsCount, desiredOutputs->size());
        }
        else
        {
            // A trick. Desired = actual, which means there is no errors:
            _daMan->copy(outputs, 0, _bpttNetDesiredOutputs, bpttIterationsCount, outputs->size());
        }
    }
}

void multilayer_perceptron::verify_training_enabled()
{
    if (!_isTrainingEnabled) throw_logic_error("This MLP cannot trained, there is no training behaviors specified.");
}

void multilayer_perceptron::ensure_training_initialized()
{
    if (!_isTrainingInitialized)
    {
        verify_training_enabled();
        _initLearningFunc();
        _isTrainingInitialized = true;
    }
}

void multilayer_perceptron::training(const data_array_ptr& input, const data_array_ptr& desiredOutputs, const data_array_ptr& actualOutputs)
{
    supervised_batch batch;
    batch.push_back(input, desiredOutputs, actualOutputs);
    training(batch);
}

void multilayer_perceptron::training(const supervised_sample_entry& sampleEntry)
{
    supervised_batch batch;
    batch.push_back(sampleEntry);
    training(batch);
}

void multilayer_perceptron::training(const supervised_sample& sample)
{
    supervised_batch batch;
    batch.push_back(sample);
    training(batch);
}

void multilayer_perceptron::training(supervised_batch& batch)
{
    ensure_training_initialized();
    if (_doBackpropagate)
    {
        if (_doFFBP)
        {
            feed_forward_training(batch);
        }
        else
        {
            bppt_training(batch);
        }
    }
    else if (_doRTLR)
    {
        rtlr_training(batch);
    }
    else
    {
        global_optimization_training(batch);
    }
}

void multilayer_perceptron::feed_forward_training(supervised_batch& batch)
{
    assert(_isGradientsCalculated);

    // Feed Forward:

    // Start batch:
    for (auto& sample : batch.samples())
    {
        auto& entry = sample.entries().front();

        // Compute forward:
        compute_sample_entry(entry.input(), entry.actual_output(), entry.desired_output());

        // Backpropagate:
        _trainFunc(0, nf::gradient_computation_formula::ff, 0);

        // Do Gradient based online algo step
        _onlineLearningFunc(_globalOnlineErrors);
    }

    // Do batch algos
    _offlineLearningFunc(batch.samples().size(), _globalOfflineErrors);
    zero_gradient_sums();
    zero_global_offline_errors();
}

void multilayer_perceptron::bppt_training(supervised_batch& batch)
{
    assert(_doBPTT);
    assert(_isGradientsCalculated);

    throw_not_implemented();
}

void multilayer_perceptron::rtlr_training(supervised_batch& batch)
{
    assert(_doRTLR);
    assert(_isGradientsCalculated);

    // Start batch:
    idx_t sampleIndex = 0;
    for (auto& sample : batch.samples())
    {
        if (sample.entries().size() <= 1) throw_logic_error("Recurrent networks cannot be trained by using feed forward samples.");

        idx_t lastEntryIndex = sample.entries().size() - 1;

        // Compute forward + gradients:
        data_array_ptr noActualOutput;
        const data_array_ptr* actualOutputs = null;
        idx_t entryIndex = 0;
        for (auto& entry : sample.entries())
        {
            if (actualOutputs == null) actualOutputs = &(find_actual_output(sample, entryIndex));

            // Compute forward:
            compute_sample_entry(entry.input(), actualOutputs ? *actualOutputs : noActualOutput, entry.desired_output());

            if (_netDesiredOutputs != null)
            {
                // We have and error (gradients are calculated in the forward phase):

                // Do Gradient based online algo step
                _onlineLearningFunc(_globalOnlineErrors);
            }

            if (actualOutputs && actualOutputs->get() == entry.actual_output().get())
            {
                // We should use an other output vector in the next iteration
                actualOutputs = null;
            }

            entryIndex++;
        }

        if (sampleIndex == batch.samples().size() - 1)
        {
            // This is the last sample:

            // Do batch algos
            _offlineLearningFunc(batch.samples().size(), _globalOfflineErrors);
            zero_gradient_sums();
            zero_global_offline_errors();
        }

        sampleIndex++;

        // New sample = new memory:
        zero_outputs();
        _rtlr->zero();
    }
}

void multilayer_perceptron::global_optimization_training(supervised_batch& batch)
{
    throw_not_implemented();
}

const data_array_ptr& multilayer_perceptron::find_actual_output(supervised_sample& sample, idx_t entryIndex)
{
    auto& entries = sample.entries();
    idx_t size = entries.size();
    for (idx_t i = entryIndex; i < size; i++)
    {
        if (entries[i].actual_output()) return entries[i].actual_output();
    }
    throw_logic_error("Actual output data array is not found in sample.");
}

void multilayer_perceptron::zero_global_offline_errors()
{
    if (_globalOfflineErrors) context()->utils()->zero(_globalOfflineErrors);
}

void multilayer_perceptron::zero_errors()
{
    _errors.zero();
}

void multilayer_perceptron::zero_outputs()
{
    _outputs.zero();
}

void multilayer_perceptron::zero_gradients()
{
    if (_gradientsPool && _gradientsPool->is_allocated()) _gradientsPool->zero();
}

void multilayer_perceptron::zero_gradient_sums()
{
    if (_gradientSumsPool && _gradientSumsPool->is_allocated()) _gradientSumsPool->zero();
}
