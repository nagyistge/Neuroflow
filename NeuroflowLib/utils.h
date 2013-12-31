#pragma once

#include "nfdev.h"

namespace nf
{
    struct utils : virtual nf_object
    {
        virtual void randomize_uniform(device_array_ptr deviceArray, float min, float max) = 0;
        virtual void calculate_mse(supervised_batch& batch, data_array_ptr dataArray, idx_t valueIndex) const = 0;
        virtual void zero(device_array_ptr deviceArray) = 0;
    };
}