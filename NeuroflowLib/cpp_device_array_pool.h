#pragma once

#include "cpp_nfdev.h"
#include "device_array_pool.h"

namespace nf
{
    struct cpp_device_array_pool : virtual device_array_pool
    {
        ~cpp_device_array_pool();

        bool is_allocated() const override;
        device_array_ptr create_array(idx_t size) override;
        device_array2_ptr create_array2(idx_t rowSize, idx_t colSize) override;
        void allocate() override;
        void zero() override;

        float* ptr();

    private:
        idx_t endIndex = 0;
        float* internalArray = null;

        idx_t reserve(idx_t size);
    };
}