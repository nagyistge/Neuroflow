#pragma once

#include "utils.h"

namespace nf
{
    struct cpp_utils : _implements utils
    {
        void zero(device_array_ptr& deviceArray) const override;
    };
}