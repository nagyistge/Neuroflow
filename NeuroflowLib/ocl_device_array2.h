#pragma once

#include "ocl_nf.h"
#include "device_array2.h"
#include "ocl_device_array.h"

namespace nf
{
    struct ocl_device_array2 : ocl_device_array, virtual device_array2
    {
        ocl_device_array2(const cl::Buffer& buffer, idx_t size1);
        ocl_device_array2(const ocl_device_array_pool_ptr& pool, idx_t beginIndex, idx_t size1, idx_t size2);

        idx_t size1() const override;
        idx_t size2() const override;

    private:
        idx_t _size1;
    };
}