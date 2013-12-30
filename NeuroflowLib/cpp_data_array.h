#pragma once

#include "cpp_nf.h"
#include "data_array.h"
#include "cpp_device_array.h"

namespace nf
{
    struct cpp_data_array : cpp_device_array, _implements data_array
    {
        cpp_data_array(float* internalArray, idx_t arraySize, bool isReadOnly);

        bool is_read_only() const override;
        concurrency::task<void> read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex) const override;
        concurrency::task<void> write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex) override;

    private:
        bool isReadOnly;
    };
}