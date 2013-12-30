#pragma once

#include "cpp_nf.h"
#include "device_array.h"

namespace nf
{
    struct cpp_device_array : _implements device_array
    {
        cpp_device_array(float* internalArray, idx_t arraySize);
        cpp_device_array(idx_t arraySize);
        cpp_device_array(const cpp_device_array_pool_ptr& pool, idx_t beginIndex, idx_t arraySize);
        ~cpp_device_array();

        idx_t size() const override;
        float* ptr() const;

    private:
        float* internalArray = null;
        cpp_device_array_pool_ptr pool;
        idx_t beginIndex = 0, arraySize = 0;
    };
}