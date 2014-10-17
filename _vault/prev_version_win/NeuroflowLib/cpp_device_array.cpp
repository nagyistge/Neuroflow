#include "stdafx.h"
#include "cpp_device_array.h"
#include "cpp_device_array_pool.h"

USING

cpp_device_array::cpp_device_array(float* internalArray, idx_t arraySize) :
_ptr(internalArray),
_arraySize(arraySize)
{
    assert(_ptr != null && _arraySize > 0);
}

cpp_device_array::cpp_device_array(idx_t arraySize) :
_ptr(new float[arraySize]),
_arraySize(arraySize)
{
    assert(_arraySize > 0);
    memset(_ptr, 0, _arraySize * sizeof(float));
}

cpp_device_array::cpp_device_array(const cpp_device_array_pool_ptr& pool, idx_t beginIndex, idx_t arraySize) :
_pool(pool),
_beginIndex(beginIndex),
_arraySize(arraySize)
{
    assert(_arraySize > 0);
    assert(_beginIndex >= 0);
    assert(_pool != null);
}

cpp_device_array::~cpp_device_array()
{
    if (_pool == null) delete[] _ptr;
}

idx_t cpp_device_array::size() const
{
    return _arraySize;
}

float* cpp_device_array::ptr() 
{
    if (_ptr == null && _pool != null) _ptr = _pool->ptr() + _beginIndex;
    return _ptr;
}

std::string cpp_device_array::dump()
{
    stringstream s;
    float* p = ptr();
    if (p != null)
    {
        for (idx_t i = 0; i < _arraySize; i++) s << to_string(p[i]) << " ";
    }
    return s.str();
}