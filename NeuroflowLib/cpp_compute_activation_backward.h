#pragma once

#include "nfdev.h"

namespace nf
{
    struct cpp_compute_activation_backward
    {
        void compute(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf) const;
        void compute_last(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf) const;
        void compute_inner(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf) const;

    private:
        inline static float sigmoid_deriv(float value, float alpha);
    };
}