#pragma once

#include "nfdev.h"
#include "layer_behavior.h"

namespace nf
{
    struct learning_behavior : layer_behavior
    {
        unsigned group_id;

        bool props_equals(const layer_behavior_ptr& other) const override;
    };
}