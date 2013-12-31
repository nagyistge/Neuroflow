#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array_pool : virtual nf_object
    {
        virtual bool is_allocated() const = 0;
        virtual device_array_ptr create_array(idx_t size) = 0;
        virtual device_array2_ptr create_array2(idx_t rowSize, idx_t colSize) = 0;
        virtual void allocate() = 0;
        virtual void zero() = 0;
    };
}