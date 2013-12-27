#pragma once

#include "utils.h"

namespace nf
{
    struct cpp_utils : utils
    {
        void zero(device_array_ptr& deviceArray) const override;
    };
}