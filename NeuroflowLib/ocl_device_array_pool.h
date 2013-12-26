#pragma once

#include "ocl_nf.h"
#include "device_array_pool.h"

namespace nf
{
    struct ocl_device_array_pool : device_array_pool
    {
        friend struct ocl_device_array;

        ocl_device_array_pool(const ocl_device_array_management_ptr& deviceArrayMan, const ocl_utils_ptr& utils);

        bool is_allocated() const override;
        device_array_ptr create_array(idx_t size) override;
        device_array2_ptr create_array2(idx_t rowSize, idx_t colSize) override;
        void allocate() override;
        void zero() override;

    private:
        cl::Buffer buffer;
        idx_t endIndex;
        ocl_device_array_management_ptr deviceArrayMan;
        ocl_utils_ptr utils;

        idx_t reserve(idx_t size);
        cl::Buffer create_sub_buffer(unsigned beginOffset, unsigned size);
    };
}