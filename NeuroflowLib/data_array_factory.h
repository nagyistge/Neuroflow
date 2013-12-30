#pragma once

#include "nf.h"

namespace nf
{
    _interface data_array_factory : virtual nf_object
    {
        virtual data_array_ptr create(idx_t size, float fill) = 0;
        virtual data_array_ptr create(float* values, idx_t beginPos, idx_t size) = 0;
        virtual data_array_ptr create_read_only(float* values, idx_t beginPos, idx_t size) = 0;
    };
}