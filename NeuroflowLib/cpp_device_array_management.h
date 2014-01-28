#pragma once

#include "cpp_nfdev.h"
#include "device_array_management.h"

namespace nf
{
    struct cpp_device_array_management : virtual device_array_management
    {
        device_array_ptr create_array(bool copyOptimized, idx_t size) override;
        device_array2_ptr create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize) override;
        void copy(device_array_ptr from, idx_t fromIndex, device_array_ptr to, idx_t toIndex, idx_t size) override;
        device_array_pool_ptr create_pool(bool copyOptimized) override;
    };
}