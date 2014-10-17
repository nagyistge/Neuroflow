#pragma once

#include "cpp_nfdev.h"
#include "compute_activation.h"
#include "cpp_compute_activation_forward.h"
#include "cpp_compute_activation_backward.h"
#include "cpp_compute_activation_gradients_rtlr.h"

namespace nf
{
    struct cpp_compute_activation : virtual compute_activation
    {
        nf_object_ptr create_operation_context() override;
        
        void compute_forward(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) override
        {
            forward.compute(context, nodes, offset);
        }

        void compute_backward(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) override
        {
            backward.compute(context, nodes, offset, gcf, internalIterationCount);
        }

        void compute_gradients_rtlr(const nf_object_ptr& context, const rtlr_layer_info_groups_t& inputLayerInfos, const device_array_collection_t& netValueDerivates, const rtlr_computation_data& data, const device_array2_ptr& pValuesOfWeights, device_array* outputs, device_array* desiredOutputs, sequence_marker seqMark) override
        {
            rtlr.compute_gradients_rtlr(context, inputLayerInfos, netValueDerivates, data, pValuesOfWeights, outputs, desiredOutputs, seqMark);
        }

    private:
        cpp_compute_activation_forward forward;
        cpp_compute_activation_backward backward;
        cpp_compute_activation_gradients_rtlr rtlr;
    };
}