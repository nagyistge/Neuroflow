#pragma once

#include "nfdev.h"
#include "rtlr_layer_info.h"

namespace nf
{
    struct rtlr 
    {
        void initialize(multilayer_perceptron* mlp);
        void compute_gradients(device_array* desiredOutputs);
        void zero();

    private:
        typedef std::vector<std::function<void(device_array*, device_array*)>> step_functions_t;

        multilayer_perceptron* _mlp = null;
        idx_t _uLayersCount = 0;
        idx_t _maxULayerSize = 0;
        rtlr_layer_info_groups_t _inputLayerInfos;
        device_array_collection_t _netValueDerivates;
        device_array_pool_ptr _pValuesPool;
        device_array2_groups_t _pValues;
        step_functions_t _steps;

        device_array2_ptr create_p_values_for_weights(const device_array_ptr& weights);
        void compute_gradients(
            idx_t iLayerIndex, 
            idx_t jLayerIndex,
            const device_array2_ptr& pValuesOfWeights, 
            device_array* outputs,
            device_array* desiredOutputs,
            idx_t computationIndex,
            sequence_marker seqMark);
    };
}