#pragma once

#include "nfdev.h"

namespace nf
{
    struct compute_activation : virtual nf_object
    {
        virtual nf_object_ptr create_operation_context() = 0;
    };
}