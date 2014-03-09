#pragma once

#include "nfdev.h"
#include "weighted_inputs.h"
#include "activation_description.h"

namespace nf
{
    struct mlp_forward_node
    {
        activation_description activation;
        std::vector<weighted_inputs> in;
        get_device_array_ptr_t out;
        device_array_ptr bias;
        device_array_ptr derivates;
        
        idx_t size() const;
    };
};