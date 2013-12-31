#include "stdafx.h"
#include "ocl_device_array2.h"

USING;

ocl_device_array2::ocl_device_array2(const cl::Buffer& buffer, idx_t size1) :
ocl_device_array(buffer),
_size1(size1)
{
}

ocl_device_array2::ocl_device_array2(const ocl_device_array_pool_ptr& pool, idx_t beginIndex, idx_t size1, idx_t size2) :
ocl_device_array(pool, beginIndex, size1 * size2),
_size1(size1)
{
}

idx_t ocl_device_array2::size1() const
{
    return _size1;
}

idx_t ocl_device_array2::size2() const
{
    return size() / _size1;
}