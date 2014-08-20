#pragma once

#include "ocl_nfdev.h"
#include "../device_array.h"

namespace nf
{
    struct ocl_device_array : virtual device_array
    {
        ocl_device_array(const cl::Buffer& buffer);
        ocl_device_array(const ocl_device_array_pool_ptr& pool, idx_t beginOffset, idx_t arraySize);

        idx_t size() const override;
        std::string dump() override 
        {
            throw_not_implemented();
        }
        const cl::Buffer& buffer();

    private:
        cl::Buffer _buffer;
        idx_t beginIndex = 0;
        idx_t arraySize = 0;
        ocl_device_array_pool_ptr pool;
    };
}
