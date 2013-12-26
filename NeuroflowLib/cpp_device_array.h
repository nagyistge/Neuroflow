#pragma once

#include "cpp_nf.h"
#include "device_array.h"

namespace nf
{
    struct cpp_device_array : virtual device_array
    {
        cpp_device_array(float* internalArray, ::size_t arraySize);
        cpp_device_array(::size_t arraySize);
        cpp_device_array(const cpp_device_array_pool_ptr& pool, ::size_t beginIndex, ::size_t arraySize);

        ::size_t size() const override;
        float* ptr() const;

    private:
        float* internalArray = null;
        cpp_device_array_pool_ptr pool;
        ::size_t beginIndex = 0, arraySize = 0;
    };
}