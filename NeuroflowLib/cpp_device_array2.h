#pragma once

#include "cpp_nfdev.h"
#include "device_array2.h"
#include "cpp_device_array.h"

namespace nf
{
    struct cpp_device_array2 : cpp_device_array, virtual device_array2
    {
        cpp_device_array2(idx_t size1, idx_t size2);
        cpp_device_array2(const cpp_device_array_pool_ptr pool, idx_t beginIndex, idx_t size1, idx_t size2);
        
        idx_t size1() const override;
        idx_t size2() const override;

    private:
        idx_t _size1 = 0, _size2 = 0;
    };
}