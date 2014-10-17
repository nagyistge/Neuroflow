#pragma once

#include "nfdev.h"

namespace nf
{
    struct rtlr_computation_data
    {
        get_device_array_ptr_t inputs;
        device_array2_ptr gradients;
        device_array2_ptr gradient_sums;
        device_array_ptr bias_gradients;
        device_array_ptr bias_gradient_sums;
        idx_t i_layer_index;
        idx_t j_layer_index;
        idx_t max_u_layer_size;
        idx_t u_layers_count;
    };
}