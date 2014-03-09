#include "stdafx.h"
#include "rtlr.h"
#include "multilayer_perceptron.h"
#include "device_array.h"
#include "device_array2.h"
#include "layer.h"
#include "device_array2_group.h"
#include "device_array_group.h"
#include "computation_context.h"
#include "device_array_management.h"
#include "device_array_pool.h"
#include "rtlr_computation_data.h"
#include "compute_activation.h"

USING
namespace ph = std::placeholders;

void rtlr::initialize(multilayer_perceptron* mlp)
{
    assert(mlp);

    _mlp = mlp;
    _netValueDerivates = _mlp->_netValueDerivates.get_arrays() | to_vector();
    _pValuesPool = _mlp->context()->device_array_management()->create_pool(false);
    _uLayersCount = _mlp->_layers.size() - 1;
    _maxULayerSize = _mlp->_layers |
    where([](row_numbered<layer_ptr>& l) { return l.row_num() != 0; }) |
    max_value([](row_numbered<layer_ptr>& l) { return l.row_num(); });
    
    for (idx_t lidx = 1; lidx < _mlp->_layers.size(); lidx++)
    {
        _inputLayerInfos.emplace_back();
        _pValues.emplace_back();
        auto& layer = _mlp->_layers[lidx].value();
        _pValues.back().emplace_back(create_p_values_for_weights(_mlp->_biases.get(lidx)));
        for (auto& inputLayer : layer->input_layers())
        {
            idx_t iidx = _mlp->get_layer_index(inputLayer);
            _inputLayerInfos.back().emplace_back();
            auto& info = _inputLayerInfos.back().back();
            info.index = iidx - 1;
            info.size = inputLayer->size();
            info.weights = _mlp->_weights.get(make_pair(iidx, lidx));
            info.is_element_of_u = inputLayer != _mlp->_layers[0].value();
            _pValues.back().emplace_back(info.weights);
        }
    }
}

device_array2_ptr rtlr::create_p_values_for_weights(const device_array_ptr& weights)
{
    idx_t xSize = _uLayersCount * _maxULayerSize;
    idx_t ySize = weights->size();
    return _pValuesPool->create_array2(ySize, xSize);
}

void rtlr::compute_gradients(device_array* desiredOutputs)
{
    auto& outputs = _mlp->_netOutputs;
    idx_t computationIndex = 0;
    idx_t layersCount = _mlp->_layers.size();
    for (idx_t lidx = 1; lidx < layersCount; lidx++)
    {
        idx_t iLayerIndex = lidx - 1;
        auto& pValuesOfLayer = _pValues[iLayerIndex];
        idx_t pValuesOfLayersCount = pValuesOfLayer.size();
        for (idx_t jLayerIndex = 0; jLayerIndex < pValuesOfLayersCount; jLayerIndex++)
        {
            // 0: Bias
            // 1..: Weights
            auto& pValuesOfWeights = pValuesOfLayer[jLayerIndex];
            sequence_marker seqMark = sequence_marker::inner;
            if (lidx == 1 && jLayerIndex == 0) seqMark = sequence_marker::begin;
            else if (lidx == layersCount - 1 && jLayerIndex == pValuesOfLayersCount - 1) seqMark = sequence_marker::end;
            compute_gradients(iLayerIndex, jLayerIndex, pValuesOfWeights, outputs, desiredOutputs, computationIndex++, seqMark);
        }
    }
}

void rtlr::compute_gradients(
    idx_t iLayerIndex,
    idx_t jLayerIndex,
    const device_array2_ptr& pValuesOfWeights,
    device_array* outputs,
    device_array* desiredOutputs,
    idx_t computationIndex,
    sequence_marker seqMark)
{
    // jLayerIndex: 0: Bias, 1..: Weights

    if (_steps.size() <= computationIndex)
    {
        _steps.resize(computationIndex + 1);
        bool forBias = jLayerIndex == 0;
        rtlr_computation_data data;
        data.u_layers_count = _uLayersCount;
        data.max_u_layer_size = _maxULayerSize;
        idx_t iLayerIndexN = iLayerIndex + 1;
        auto& iLayer = _mlp->_layers[iLayerIndexN].value();
        data.i_layer_index = iLayerIndex;
        data.j_layer_index = jLayerIndex;
        if (forBias)
        {
            assert(jLayerIndex == 0);
            _mlp->_biasGradients.try_get(iLayerIndexN, data.bias_gradients);
            _mlp->_biasGradientSums.try_get(iLayerIndexN, data.bias_gradient_sums);
        }
        else
        {
            assert(jLayerIndex > 0);
            idx_t jLayerIndexN = jLayerIndex - 1;
            auto& inputLayerOfILayer = iLayer->get_input_layer(jLayerIndexN);
            idx_t inputLayerOfILayerIndex = _mlp->get_layer_index(inputLayerOfILayer);
            auto weightKey = make_pair(inputLayerOfILayerIndex, iLayerIndexN);
            data.inputs = [=]() { return _mlp->get_net_values(inputLayerOfILayerIndex); };
            _mlp->_gradients.try_get(weightKey, data.gradients);
            _mlp->_gradientSums.try_get(weightKey, data.gradient_sums);
        }

        assert(!(data.bias_gradients == null && data.bias_gradient_sums == null && data.gradients == null && data.gradient_sums == null));

        auto comp = _mlp->context()->compute_activation();
        _steps[computationIndex] =
        std::bind(
        [=](const nf_object_ptr& context, const rtlr_computation_data& data, device_array* outputs, device_array* desiredOutputs)
        {
            comp->compute_gradients_rtlr(
                context, 
                this->_inputLayerInfos, 
                this->_netValueDerivates, 
                data, 
                pValuesOfWeights, 
                outputs, 
                desiredOutputs, 
                seqMark);
        },
        move(comp->create_operation_context()),
        move(data),
        ph::_1,
        ph::_2);
    }

    _steps[computationIndex](outputs, desiredOutputs);
}