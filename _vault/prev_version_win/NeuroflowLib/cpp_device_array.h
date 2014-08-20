#pragma once

#include "cpp_nfdev.h"
#include "device_array.h"

namespace nf
{
    struct cpp_device_array : virtual device_array
    {
        cpp_device_array(float* internalArray, idx_t arraySize);
        cpp_device_array(idx_t arraySize);
        cpp_device_array(const cpp_device_array_pool_ptr& pool, idx_t beginIndex, idx_t arraySize);
        ~cpp_device_array();

        idx_t size() const override;
        std::string dump() override;
        float* ptr();

    private:
        float* _ptr = null;
        cpp_device_array_pool_ptr _pool;
        idx_t _beginIndex = 0, _arraySize = 0;
    };
}