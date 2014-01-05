#pragma once

#include "nfdev.h"

namespace nf
{
    struct multilayer_perceptron_props
    {
        gradient_computation_method gradient_computation_method = gradient_computation_method::feed_forward;
    };
}