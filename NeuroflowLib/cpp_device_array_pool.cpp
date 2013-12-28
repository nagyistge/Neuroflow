#include "stdafx.h"
#include "cpp_device_array_pool.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"

using namespace std;
using namespace nf;

float* cpp_device_array_pool::ptr()
{
    allocate();
    assert(internalArray != null);
    assert(is_allocated());
    return internalArray;
}

bool cpp_device_array_pool::is_allocated() const
{
    return internalArray != null;
}

device_array_ptr cpp_device_array_pool::create_array(idx_t size)
{
    return make_shared<cpp_device_array>(shared_this<cpp_device_array_pool>(), reserve(size), size);
}

device_array2_ptr cpp_device_array_pool::create_array2(idx_t rowSize, idx_t colSize)
{
    return make_shared<cpp_device_array2>(shared_this<cpp_device_array_pool>(), reserve(rowSize * colSize), rowSize, colSize);
}

void cpp_device_array_pool::allocate()
{
    if (endIndex == 0) throw_logic_error("There is no allocated memory in the pool.");
    if (!is_allocated()) internalArray = new float[endIndex];
}

void cpp_device_array_pool::zero()
{
    if (!is_allocated()) throw_logic_error("Cannot zero out an unallocated pool.");
    memset(internalArray, 0, endIndex * sizeof(float));
}

idx_t cpp_device_array_pool::reserve(idx_t size)
{
    if (is_allocated()) throw_logic_error("Cannot reserve memory in an already allocated pool.");
    idx_t beginIndex = endIndex;
    endIndex += size;
    return beginIndex;
}