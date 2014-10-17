#pragma once
#include "nfdev.h"

namespace nf
{
    struct cc_init_pars : virtual nf_object
    {
        boost::optional<unsigned long> random_seed;
    };
}