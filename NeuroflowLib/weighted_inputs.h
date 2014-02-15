#pragma once

#include "nfdev.h"

namespace nf
{
    struct weighted_inputs
    {
        get_device_array_ptr_t inputs;
        device_array2_ptr weights;
    };
}