#pragma once
#include "ocl_nfdev.h"
#include "../cc_init_pars.h"

namespace nf
{
    struct ocl_cc_init_pars : cc_init_pars
    {
        unsigned max_connection_count = 4;
        unsigned max_layer_count = 4;
    };
}
