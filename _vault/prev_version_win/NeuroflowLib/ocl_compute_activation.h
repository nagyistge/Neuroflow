#pragma once

#include "ocl_nfdev.h"
#include "compute_activation.h"
#include "weak_contexted.h"

namespace nf
{
    struct ocl_compute_activation : weak_contexted<ocl_computation_context>, virtual compute_activation
    {
        ocl_compute_activation(const ocl_computation_context_wptr& context);

        nf_object_ptr create_operation_context() override;
        
        void compute_forward(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) override
        {
            throw_not_implemented();
        }
        
        void compute_backward(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) override 
        {
            throw_not_implemented();
        }

        void compute_gradients_rtlr(const nf_object_ptr& context, const rtlr_layer_info_groups_t& inputLayerInfos, const device_array_collection_t& netValueDerivates, const rtlr_computation_data& data, const device_array2_ptr& pValuesOfWeights, device_array* outputs, device_array* desiredOutputs, sequence_marker seqMark) override
        {
            throw_not_implemented();
        }
    };
}