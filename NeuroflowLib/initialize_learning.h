#pragma once

#include "nfdev.h"
#include "learning_impl.h"

namespace nf
{
    struct initialize_learning : virtual learning_impl
    {
        virtual void initialize(const device_array_collection_t& weights) = 0;
    };
}