#pragma once
#include "nfdev.h"

namespace nf
{
    struct mlp_init_pars : virtual nf_object
    {
        nf::gradient_computation_method gradient_computation_method = nf::gradient_computation_method::feed_forward;
        unsigned max_bptt_iterations = 4;
    };
}