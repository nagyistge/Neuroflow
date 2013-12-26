#pragma once

#include "cpp_nf.h"
#include "device_array_management.h"

namespace nf
{
    struct cpp_device_array_management : device_array_management
    {
        device_array_ptr create_array(bool copyOptimized, ::size_t size) override;
        device_array2_ptr create_array2(bool copyOptimized, ::size_t rowSize, ::size_t colSize) override;
        void copy(device_array_ptr from, ::size_t fromIndex, device_array_ptr to, ::size_t toIndex, ::size_t size) override;
        device_array_pool_ptr create_pool() override;
    };
}