#pragma once

#include "ocl_nf.h"

namespace nf
{
    struct ocl_internal_context : nf_object
    {
        const cl::Context cl_context() const;
        const cl::CommandQueue cl_queue() const;
    };
}