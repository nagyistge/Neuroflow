#include "stdafx.h"
#include "cpp_device_array2.h"

USING

cpp_device_array2::cpp_device_array2(idx_t size1, idx_t size2) :
cpp_device_array(size1 * size2),
_size1(size1),
_size2(size2)
{
    assert(size1 > 0);
    assert(size2 > 0);
}

cpp_device_array2::cpp_device_array2(const cpp_device_array_pool_ptr& pool, idx_t beginIndex, idx_t size1, idx_t size2) :
cpp_device_array(pool, beginIndex, size1 * size2),
_size1(size1),
_size2(size2)
{
    assert(size1 > 0);
    assert(size2 > 0);
    assert(beginIndex >= 0 && beginIndex < size());
}

idx_t cpp_device_array2::size1() const
{
    return _size1;
}

idx_t cpp_device_array2::size2() const
{
    return _size2;
}