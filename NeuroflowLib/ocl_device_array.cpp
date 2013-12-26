#include "stdafx.h"
#include "ocl_device_array.h"
#include "ocl_device_array_pool.h"

using namespace std;
using namespace nf;
using namespace cl;

ocl_device_array::ocl_device_array(const cl::Buffer& buffer) :
_buffer(buffer),
arraySize(buffer.getInfo<CL_MEM_SIZE>() / sizeof(float))
{
}

ocl_device_array::ocl_device_array(const ocl_device_array_pool_ptr& pool, idx_t beginIndex, idx_t arraySize) :
beginIndex(beginIndex),
arraySize(arraySize),
pool(pool)
{
}

idx_t ocl_device_array::size() const
{
    return arraySize;
}

cl::Buffer& ocl_device_array::buffer()
{
    if (_buffer() == null)
    {
        assert(pool != null);
        _buffer = pool->create_sub_buffer(beginIndex, arraySize);
    }
    return _buffer;
}