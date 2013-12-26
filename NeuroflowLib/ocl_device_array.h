#pragma once

#include "ocl_nf.h"
#include "device_array.h"

namespace nf
{
    struct ocl_device_array : virtual device_array
    {
        ocl_device_array(const cl::Buffer& buffer);
        ocl_device_array(const ocl_device_array_pool_ptr& pool, size_t beginOffset, size_t size);

        size_t size() const override;

    private:
        size_t beginIndex = 0;
        size_t arraySize = 0;
        ocl_device_array_pool_ptr pool;
    };
}