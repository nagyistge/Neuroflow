#pragma once

#include "nfdev.h"

namespace nf
{
    struct data_array_factory : virtual nf_object
    {
        virtual data_array_ptr create(idx_t size, float fill = 0.0f) = 0;
        virtual data_array_ptr create(const float* values, idx_t beginPos, idx_t size) = 0;
        virtual data_array_ptr create_const(const float* values, idx_t beginPos, idx_t size) = 0;
    };
}
