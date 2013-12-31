#pragma once

#include "nf.h"

namespace nf
{
    struct data_array_factory : virtual nf_object
    {
        virtual data_array_ptr create(idx_t size, float fill) = 0;
        virtual data_array_ptr create(float* values, idx_t beginPos, idx_t size) = 0;
        virtual data_array_ptr create_const(float* values, idx_t beginPos, idx_t size) = 0;
    };
}