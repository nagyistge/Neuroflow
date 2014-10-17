#pragma once

#include "nfdev.h"

namespace nf
{
    struct rtlr_layer_info
    {
        idx_t index;
        device_array2_ptr weights;
        idx_t size;
        bool is_element_of_u;
    };
}