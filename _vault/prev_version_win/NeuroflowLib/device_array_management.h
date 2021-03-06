#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array_management : virtual nf_object
    {
        virtual device_array_ptr create_array(bool copyOptimized, idx_t size) = 0;
        virtual device_array2_ptr create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize) = 0;
        virtual void copy(const device_array_ptr& from, idx_t fromIndex, const device_array_ptr& to, idx_t toIndex, idx_t size) = 0;
        virtual device_array_pool_ptr create_pool(bool copyOptimized) = 0;
    };
}