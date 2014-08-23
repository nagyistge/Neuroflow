#pragma once

#include "../nfdev.h"

namespace nf
{
    struct cpp_compute_activation_backward
    {
        friend struct cpp_compute_activation_forward;

        void compute(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) const;
        void compute_last(const mlp_backward_node& node, idx_t offset) const;
        void compute_inner(const mlp_backward_node& node, idx_t offset) const;
        void compute_gradients(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) const;
        void compute_gradients_ff(const mlp_backward_node& node, idx_t offset) const;
        void compute_gradients_bpttp1(const mlp_backward_node& node, idx_t offset) const;
        void compute_gradients_bpttp2(const mlp_backward_node& node, idx_t offset, idx_t internalIterationCount) const;

    private:
        inline static float sigmoid_deriv(float value, float alpha)
        {
            //return alpha * (1.0f - value * value) / 2.0f; // Logistics
            //return alpha * (1.0f - (value * value)); // Tanh
            return alpha * 1.0f / ((1.0f + abs(value * alpha)) * (1.0f + abs(value * alpha))); // Elliot
        }
    };
}
