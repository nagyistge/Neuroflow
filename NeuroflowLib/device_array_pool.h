#pragma once

#include "nf.h"

namespace nf
{
    struct device_array_pool : virtual nf_object
    {
        virtual bool is_allocated() const = 0;
        virtual device_array_ptr create_array(size_t size) = 0;
        virtual device_array2_ptr create_array2(size_t rowSize, size_t colSize) = 0;
        virtual void allocate() = 0;
        virtual void zero() = 0;
    };
}