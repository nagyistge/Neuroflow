#include "stdafx.h"
#include "cpp_device_array.h"
#include "cpp_device_array_pool.h"

using namespace std;
using namespace nf;

cpp_device_array::cpp_device_array(float* internalArray, idx_t arraySize) :
internalArray(internalArray),
arraySize(arraySize)
{
    assert(internalArray != null && arraySize > 0);
}

cpp_device_array::cpp_device_array(idx_t arraySize) :
internalArray(new float[arraySize]),
arraySize(arraySize)
{
    assert(arraySize > 0);
    memset(internalArray, 0, arraySize * sizeof(float));
}

cpp_device_array::cpp_device_array(const cpp_device_array_pool_ptr& pool, idx_t beginIndex, idx_t arraySize) :
pool(pool),
beginIndex(beginIndex),
arraySize(arraySize)
{
    assert(arraySize > 0);
    assert(beginIndex >= 0);
    assert(pool != null);
}

idx_t cpp_device_array::size() const
{
    return arraySize;
}

float* cpp_device_array::ptr() const
{
    return pool == null ? internalArray : pool->ptr() + beginIndex;
}