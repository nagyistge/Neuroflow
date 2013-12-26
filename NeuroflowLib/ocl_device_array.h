#pragma once

#include "ocl_nf.h"
#include "device_array.h"

namespace nf
{
    struct ocl_device_array : virtual device_array
    {
        ocl_device_array(const cl::Buffer& buffer);
        ocl_device_array(const ocl_device_array_pool_ptr& pool, ::size_t beginOffset, ::size_t arraySize);

        ::size_t size() const override;
        cl::Buffer& buffer();

    private:
        cl::Buffer _buffer;
        ::size_t beginIndex = 0;
        ::size_t arraySize = 0;
        ocl_device_array_pool_ptr pool;
    };
}