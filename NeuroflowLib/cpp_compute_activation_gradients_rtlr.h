#pragma once

#include "cpp_nfdev.h"

namespace nf
{
    struct cpp_compute_activation_gradients_rtlr
    {
        void compute_gradients_rtlr(const nf_object_ptr& context, const rtlr_layer_info_groups_t& inputLayerInfos, const device_array_collection_t& netValueDerivates, const rtlr_computation_data& data, const device_array2_ptr& pValuesOfWeights, device_array* outputs, device_array* desiredOutputs, sequence_marker seqMark);

    private:
        inline static float* get_p_values_ptr(float* pPValuesOfWeights, int ijValueIndex, const rtlr_computation_data& data, idx_t kLayerIndex);
    };
}