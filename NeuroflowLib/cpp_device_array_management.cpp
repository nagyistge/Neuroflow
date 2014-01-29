#include "stdafx.h"
#include "cpp_conv.h"
#include "cpp_device_array_management.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "cpp_device_array_pool.h"

USING

device_array_ptr cpp_device_array_management::create_array(bool copyOptimized, idx_t size)
{
    verify_arg(size > 0, "size");

    return make_shared<cpp_device_array>(size);
}

device_array2_ptr cpp_device_array_management::create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize)
{
    verify_arg(rowSize > 0, "rowSize");
    verify_arg(colSize > 0, "colSize");

    return make_shared<cpp_device_array2>(rowSize, colSize);
}

void cpp_device_array_management::copy(const device_array_ptr& from, idx_t fromIndex, const device_array_ptr& to, idx_t toIndex, idx_t size)
{
    verify_arg(fromIndex >= 0, "fromIndex");
    verify_arg(toIndex >= 0, "toIndex");
    verify_arg(size >= 0, "size");

    auto fromCpp = to_cpp(from, false);
    auto toCpp = to_cpp(to, false);

    memcpy(toCpp->ptr() + toIndex, fromCpp->ptr() + fromIndex, size * sizeof(float));
}

device_array_pool_ptr cpp_device_array_management::create_pool(bool copyOptimized)
{
    return make_shared<cpp_device_array_pool>();
}