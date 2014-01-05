#pragma once

#include "nfdev.h"

namespace nf
{
    struct layer_behavior : virtual nf_object
    {
        bool equals(const layer_behavior_ptr& other) const;
        virtual bool props_equals(const layer_behavior_ptr& other) const = 0;
    };
}