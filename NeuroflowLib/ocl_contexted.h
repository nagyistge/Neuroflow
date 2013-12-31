#pragma once

#include "ocl_nfdev.h"

namespace nf
{
    struct ocl_contexted
    {
        ocl_contexted(const ocl_internal_context_ptr& context) : _context(context) 
        {
            assert(context != null);
        }

        const ocl_internal_context_ptr& context() const
        {
            return _context;
        }

    private:
        ocl_internal_context_ptr _context;
    };
}