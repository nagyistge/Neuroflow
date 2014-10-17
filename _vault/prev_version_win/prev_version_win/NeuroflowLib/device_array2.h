#pragma once

#include "device_array.h"

namespace nf
{
    struct device_array2 : virtual device_array
    {
        virtual idx_t size1() const = 0;
        virtual idx_t size2() const = 0;
    };
}