#pragma once

#include "nf.h"

namespace nf
{
    struct device_array : virtual nf_object
    {
        virtual idx_t size() const = 0;
    };
}