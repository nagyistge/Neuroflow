#pragma once

#include "nfdev.h"

namespace nf
{
    struct learning_impl : virtual nf_object
    {
        virtual void initialize() = 0;
    };
}