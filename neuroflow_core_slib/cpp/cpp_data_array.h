#pragma once

#include "cpp_nfdev.h"
#include "../data_array.h"
#include "cpp_device_array.h"

namespace nf
{
    struct cpp_data_array : cpp_device_array, virtual data_array
    {
        cpp_data_array(float* internalArray, idx_t arraySize, bool isConst);

        bool is_const() const override;
        boost::shared_future<void> read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex) override;
        boost::shared_future<void> write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex) override;

    private:
        bool isConst;

        inline void verify_if_accessible() const;
    };
}
