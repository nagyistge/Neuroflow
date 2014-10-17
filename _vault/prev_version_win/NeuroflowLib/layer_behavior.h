#pragma once

#include "nfdev.h"
#include "equatable.h"

namespace nf
{
    struct layer_behavior : virtual equatable
    {
        bool equals(const equatable& other) const override;
        virtual bool props_equals(const layer_behavior* other) const;
    };
}