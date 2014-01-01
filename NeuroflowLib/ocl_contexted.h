#pragma once

#include "ocl_nfdev.h"

namespace nf
{
    struct ocl_contexted
    {
    protected:

        ocl_contexted(const ocl_computation_context_wptr& context);

        ocl_computation_context_ptr lock_context() const;

    private:
        ocl_computation_context_wptr _context;
    };
}