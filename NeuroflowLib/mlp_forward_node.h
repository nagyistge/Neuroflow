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
        device_array_ptr out;
        device_array_ptr derivate;
    };
};