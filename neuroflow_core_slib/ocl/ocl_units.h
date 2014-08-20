#pragma once

#include "ocl_nfdev.h"
#include "../weak_contexted.h"

namespace nf
{
    struct ocl_units : weak_contexted<ocl_computation_context>
    {
        ocl_units(const ocl_computation_context_wptr& context);

        const ocl_program_unit_ptr& common() const;
        const ocl_program_unit_ptr& net() const;
        const ocl_program_unit_ptr& af() const;
        const ocl_program_unit_ptr& reduce() const;

    private:
        ocl_program_unit_ptr _common, _net, _af, _reduce;
    };
}
