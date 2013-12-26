#pragma once

#include "cpp_nf.h"
#include "device_array2.h"
#include "cpp_device_array.h"

namespace nf
{
    struct cpp_device_array2 : cpp_device_array, virtual device_array2
    {
        cpp_device_array2(int size1, int size2);
        cpp_device_array2(const cpp_device_array_pool_ptr pool, int beginIndex, int size1, int size2);
        
        size_t size1() const override;
        size_t size2() const override;

    private:
        size_t _size1 = 0, _size2 = 0;
    };
}