#pragma once

#include "nf.h"

namespace nf
{
    struct device_array_management : nf_object
    {
        virtual device_array_ptr create_array(bool copyOptimized, idx_t size) = 0;
        virtual device_array2_ptr create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize) = 0;
        virtual void copy(device_array_ptr from, idx_t fromIndex, device_array_ptr to, idx_t toIndex, idx_t size) = 0;
        virtual device_array_pool_ptr create_pool() = 0;
    };
}