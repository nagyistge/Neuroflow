#pragma once

#include "nfdev.h"
#include "learning_behavior.h"

namespace nf
{
    struct supervised_learning_behavior : learning_behavior
    {
        virtual learning_algo_optimization_type optimization_type() const = 0;
        virtual weight_update_mode weight_update_mode() const = 0;
    };
}