#include "../stdafx.h"
#include "ocl_compute_activation.h"

USING

ocl_compute_activation::ocl_compute_activation(const ocl_computation_context_wptr& context) :
weak_contexted(context)
{
}

nf_object_ptr ocl_compute_activation::create_operation_context()
{
    return null;
}
