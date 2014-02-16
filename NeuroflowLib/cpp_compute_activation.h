#pragma once

#include "cpp_nfdev.h"
#include "compute_activation.h"
#include "cpp_compute_activation_forward.h"

namespace nf
{
    struct cpp_compute_activation : virtual compute_activation
    {
        nf_object_ptr create_operation_context() override;
        void compute_forward(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) override
        {
            forward.compute(context, nodes, offset);
        }

    private:
        cpp_compute_activation_forward forward;
    };
}