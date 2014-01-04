#include "stdafx.h"
#include "ocl_contexted.h"

USING;

ocl_contexted::ocl_contexted(const ocl_computation_context_wptr& context) :
_context(context)
{
}

ocl_computation_context_ptr ocl_contexted::lock_context() const
{
    auto ptr = _context.lock();
    if (!ptr) throw_runtime_error("Internal OpenCL context is expired.");
    return ptr;
}