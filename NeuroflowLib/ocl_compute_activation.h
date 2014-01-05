#pragma once

#include "ocl_nfdev.h"
#include "compute_activation.h"
#include "weak_contexted.h"

namespace nf
{
    struct ocl_compute_activation : weak_contexted<ocl_computation_context>, virtual compute_activation
    {
        ocl_compute_activation(const ocl_computation_context_wptr& context);

        nf_object_ptr create_operation_context() override;
    };
}