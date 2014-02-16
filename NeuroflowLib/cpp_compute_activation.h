#pragma once

#include "cpp_nfdev.h"
#include "compute_activation.h"

namespace nf
{
    struct cpp_compute_activation : virtual compute_activation
    {
        nf_object_ptr create_operation_context() override;
        void compute_forward(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) override { }
    };
}