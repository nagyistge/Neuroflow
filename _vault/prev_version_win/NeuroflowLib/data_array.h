#pragma once

#include "nfdev.h"
#include "device_array.h"

namespace nf
{
    struct data_array : virtual device_array
    {
        virtual bool is_const() const = 0;
        virtual boost::shared_future<void> read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex) = 0;
        virtual boost::shared_future<void> write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex) = 0;
    };
}