#include "stdafx.h"
#include "cpp_device_array2.h"

using namespace std;
using namespace nf;

cpp_device_array2::cpp_device_array2(::size_t size1, ::size_t size2) :
cpp_device_array(size1 * size2),
_size1(size1),
_size2(size2)
{
    assert(size1 > 0);
    assert(size2 > 0);
}

cpp_device_array2::cpp_device_array2(const cpp_device_array_pool_ptr pool, ::size_t beginIndex, ::size_t size1, ::size_t size2) :
cpp_device_array(pool, beginIndex, size1 * size2),
_size1(size1),
_size2(size2)
{
    assert(size1 > 0);
    assert(size2 > 0);
    assert(beginIndex >= 0 && beginIndex < size());
}

::size_t cpp_device_array2::size1() const
{
    return _size1;
}

::size_t cpp_device_array2::size2() const
{
    return _size2;
}