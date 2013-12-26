#pragma once

#include "cpp_nf.h"
#include "device_array_pool.h"

namespace nf
{
    struct cpp_device_array_pool : device_array_pool
    {
        bool is_allocated() const override;
        device_array_ptr create_array(::size_t size) override;
        device_array2_ptr create_array2(::size_t rowSize, ::size_t colSize) override;
        void allocate() override;
        void zero() override;

        float* ptr();

    private:
        ::size_t endIndex = 0;
        float* internalArray = null;

        ::size_t reserve(::size_t size);
    };
}