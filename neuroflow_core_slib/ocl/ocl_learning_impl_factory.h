#pragma once
#include "ocl_nfdev.h"
#include "../learning_impl_factory.h"
#include "../weak_contexted.h"

namespace nf
{
    struct ocl_learning_impl_factory : weak_contexted<ocl_computation_context>, virtual learning_impl_factory
    {
        ocl_learning_impl_factory(const ocl_computation_context_wptr& context) : weak_contexted(context) { }

    protected:
        factory_map_t get_factories() override;
    };
}
