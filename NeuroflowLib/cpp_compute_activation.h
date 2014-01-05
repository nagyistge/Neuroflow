#pragma once

#include "cpp_nfdev.h"
#include "compute_activation.h"

namespace nf
{
    struct cpp_compute_activation : virtual compute_activation
    {
        nf_object_ptr create_operation_context() override;
    };
}