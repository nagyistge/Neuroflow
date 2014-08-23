#pragma once

#include "../nfdev.h"

namespace nf
{
    struct cpp_compute_activation_forward
    {
        void compute(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) const;

    private:
        inline static float sigmoid(float value, float alpha)
        {
            //return (2.0f / (1.0f + exp(-alpha * value))) - 1.0f; // Logistics
            // return (float)Math.Tanh(value * alpha); // Tanh
            return (value * alpha) / (1.0f + abs(value * alpha)); // Elliot
        }

        inline static float linear(float value, float alpha)
        {
            return nfmin(nfmax(value * alpha, -alpha), alpha);
        }
    };
}
