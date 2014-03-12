#include "stdafx.h"
#include "cpp_compute_activation_gradients_rtlr.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "rtlr_layer_info.h"
#include "rtlr_computation_data.h"

USING

void cpp_compute_activation_gradients_rtlr::compute_gradients_rtlr(const nf_object_ptr& context, const rtlr_layer_info_groups_t& inputLayerInfos, const device_array_collection_t& netValueDerivates, const rtlr_computation_data& data, const device_array2_ptr& pValuesOfWeights, device_array* outputs, device_array* desiredOutputs, sequence_marker seqMark)
{

}

void cpp_compute_activation_gradients_rtlr::set_gradients(const rtlr_computation_data& data, idx_t ijValueIndex, float gradient)
{
    if (data.gradients)
    {
        auto gradients = _fast_cast<cpp_device_array2>(data.gradients.get());
        assert(gradients);
        assert(data.j_layer_index > 0);
        assert(ijValueIndex < gradients->size());
        float* pGradients = gradients->ptr();
        pGradients[ijValueIndex] = gradient;
    }

    if (data.gradient_sums)
    {
        auto gradientSums = _fast_cast<cpp_device_array2>(data.gradient_sums.get());
        assert(gradientSums);
        assert(data.j_layer_index > 0);
        assert(ijValueIndex < gradientSums->size());
        float* pGradientSums = gradientSums->ptr();
        pGradientSums[ijValueIndex] = gradient;
    }

    if (data.bias_gradients)
    {
        auto biasGradients = _fast_cast<cpp_device_array>(data.bias_gradients.get());
        assert(biasGradients);
        assert(data.j_layer_index == 0);
        assert(ijValueIndex < biasGradients->size());
        float* pBiasGradients = biasGradients->ptr();
        pBiasGradients[ijValueIndex] = gradient;
    }

    if (data.bias_gradient_sums)
    {
        auto biasGradientSums = _fast_cast<cpp_device_array>(data.bias_gradient_sums.get());
        assert(biasGradientSums);
        assert(data.j_layer_index == 0);
        assert(ijValueIndex < biasGradientSums->size());
        float* pBiasGradientSums = biasGradientSums->ptr();
        pBiasGradientSums[ijValueIndex] = gradient;
    }
}