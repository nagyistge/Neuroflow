#pragma once

#include "nfdev.h"

namespace nf
{
    struct cpp_compute_activation_forward
    {
        void compute(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset);

    private:
        inline static float sigmoid(float value, float alpha);
        inline static float linear(float value, float alpha);
    };
}