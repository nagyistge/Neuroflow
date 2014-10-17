#pragma once

#include "nfdev.h"
#include "learning_impl.h"

namespace nf
{
    struct supervised_learning : virtual learning_impl
    {
        virtual supervised_learning_iteration_type iteration_type() const = 0;
        virtual void run(idx_t iterationCount, const device_array_ptr& error) = 0;
    };
}