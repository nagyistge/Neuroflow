#pragma once

#include "nf.h"

namespace nf
{
    struct device_array_management : virtual nf_object
    {
        virtual device_array_ptr create_array(bool copyOptimized, ::size_t size) = 0;
        virtual device_array2_ptr create_array2(bool copyOptimized, ::size_t rowSize, ::size_t colSize) = 0;
        virtual void copy(device_array_ptr from, ::size_t fromIndex, device_array_ptr to, ::size_t toIndex, ::size_t size) = 0;
        virtual device_array_pool_ptr create_pool() = 0;
    };
}