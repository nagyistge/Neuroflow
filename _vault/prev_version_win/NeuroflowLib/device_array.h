#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array : virtual nf_object
    {
        virtual idx_t size() const = 0;
        virtual std::string dump() = 0;
    };
}