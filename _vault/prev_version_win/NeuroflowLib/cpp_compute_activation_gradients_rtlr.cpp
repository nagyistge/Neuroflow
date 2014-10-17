#include "stdafx.h"
#include "cpp_compute_activation_gradients_rtlr.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "rtlr_layer_info.h"
#include "rtlr_computation_data.h"

USING

void cpp_compute_activation_gradients_rtlr::compute_gradients_rtlr(const nf_object_ptr& context, const rtlr_layer_info_groups_t& inputLayerInfos, const device_array_collection_t& netValueDerivates, const rtlr_computation_data& data, const device_array2_ptr& pValuesOfWeights, device_array* outputs, device_array* desiredOutputs, sequence_marker seqMark)
{
    float* pOutputs = null;
    if (outputs)
    {
        auto cppOutputs = _fast_cast<cpp_device_array>(outputs);
        assert(cppOutputs);
        pOutputs = cppOutputs->ptr();
    }
    
    float* pDesiredOutputs = null;
    if (desiredOutputs)
    {
        auto cppDesiredOutputs = _fast_cast<cpp_device_array>(desiredOutputs);
        assert(cppDesiredOutputs);
        pDesiredOutputs = cppDesiredOutputs->ptr();
    }
    
    float* pInputs = null;
    idx_t inputsSize = 1;
    if (data.inputs)
    {
        auto cppInputs = _fast_cast<cpp_device_array>(data.inputs());
        assert(cppInputs);
        pInputs = cppInputs->ptr();
        inputsSize = cppInputs->size();
    }
    
    assert(pValuesOfWeights);
    auto cppPValuesOfWeights = _fast_cast<cpp_device_array2>(pValuesOfWeights.get());
    assert(cppPValuesOfWeights);
    float* pPValuesOfWeights = cppPValuesOfWeights->ptr();
    idx_t pValuesOfWeightsSize1 = cppPValuesOfWeights->size1();

    float* pGradients = null;
    if (data.gradients)
    {
        auto gradients = _fast_cast<cpp_device_array2>(data.gradients.get());
        assert(gradients);
        assert(data.j_layer_index > 0);
        pGradients = gradients->ptr();
    }

    float* pGradientSums = null;
    if (data.gradient_sums)
    {
        auto gradientSums = _fast_cast<cpp_device_array2>(data.gradient_sums.get());
        assert(gradientSums);
        assert(data.j_layer_index > 0);
        pGradientSums = gradientSums->ptr();
    }

    float* pBiasGradients = null;
    if (data.bias_gradients)
    {
        auto biasGradients = _fast_cast<cpp_device_array>(data.bias_gradients.get());
        assert(biasGradients);
        assert(data.j_layer_index == 0);
        pBiasGradients = biasGradients->ptr();
    }

    float* pBiasGradientSums = null;
    if (data.bias_gradient_sums)
    {
        auto biasGradientSums = _fast_cast<cpp_device_array>(data.bias_gradient_sums.get());
        assert(biasGradientSums);
        assert(data.j_layer_index == 0);
        pBiasGradientSums = biasGradientSums->ptr();
    }

    for (idx_t ijValueIndex = 0; ijValueIndex < pValuesOfWeightsSize1; ijValueIndex++) // group Id
    {
        float gradient = 0.0f;

        idx_t iValueIndex = ijValueIndex / inputsSize;
        idx_t jValueIndex = ijValueIndex % inputsSize;

        float inputValue = pInputs ? pInputs[jValueIndex] : 1.0f;

        for (idx_t kLayerIndex = 0; kLayerIndex < data.u_layers_count; kLayerIndex++)
        {
            idx_t kLayerSize = netValueDerivates[kLayerIndex]->size();

            for (idx_t kValueIndex = 0; kValueIndex < kLayerSize; kValueIndex++)
            {
                auto layerNetValueDerivates = _fast_cast<cpp_device_array>(netValueDerivates[kLayerIndex].get());
                float* pLayerNetValueDerivates = layerNetValueDerivates->ptr();

                int outputLayerIndex = layerNetValueDerivates->size() - 1;
                bool computeGradient = kLayerIndex == outputLayerIndex && outputs != null && desiredOutputs != null;
                float* p_i_j_k_Ptr = get_p_values_ptr(pPValuesOfWeights, ijValueIndex, data, kLayerIndex);

                float sum = 0.0f;

                for (auto& lLayerInfo : inputLayerInfos[kLayerIndex])
                {
                    if (lLayerInfo.is_element_of_u)
                    {
                        assert(lLayerInfo.weights != null);

                        float* p_i_j_l_Ptr = get_p_values_ptr(pPValuesOfWeights, ijValueIndex, data, lLayerInfo.index);
                        auto weights = _fast_cast<cpp_device_array2>(lLayerInfo.weights);
                        assert(weights);
                        float* pWeights = weights->ptr();

                        for (idx_t lValueIndex = 0; lValueIndex < lLayerInfo.size; lValueIndex++)
                        {
                            idx_t widx = get_index2(lValueIndex, kValueIndex, lLayerInfo.size);
                            sum += pWeights[widx] * p_i_j_l_Ptr[lValueIndex];
                        }
                    }
                }

                if (data.i_layer_index == kLayerIndex && iValueIndex == kValueIndex) sum += inputValue;

                p_i_j_k_Ptr[kValueIndex] = pLayerNetValueDerivates[kValueIndex] * sum;

                if (computeGradient)
                {
                    assert(pOutputs);
                    assert(pDesiredOutputs);
                    gradient += (pDesiredOutputs[kValueIndex] - pOutputs[kValueIndex]) * p_i_j_k_Ptr[kValueIndex];
                }
            }
        }

        if (pGradients) pGradients[ijValueIndex] = gradient;
        if (pGradientSums) pGradientSums[ijValueIndex] += gradient;
        if (pBiasGradients) pBiasGradients[ijValueIndex] = gradient;
        if (pBiasGradientSums) pBiasGradientSums[ijValueIndex] += gradient;
    }
}

float* cpp_compute_activation_gradients_rtlr::get_p_values_ptr(float* pPValuesOfWeights, idx_t ijValueIndex, const rtlr_computation_data& data, idx_t kLayerIndex)
{
    return pPValuesOfWeights + (ijValueIndex * (data.u_layers_count * data.max_u_layer_size) + kLayerIndex * data.max_u_layer_size);
}
