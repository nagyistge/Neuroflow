#pragma once

#include "nfdev.h"
#include "weighted_inputs.h"
#include "activation_description.h"

namespace nf
{
    struct mlp_backward_node
    {
        activation_description activation;
        get_device_array_ptr_t in;
        get_device_array_ptr_t out;
        device_array_ptr error;

        idx_t size() const;
    };
};