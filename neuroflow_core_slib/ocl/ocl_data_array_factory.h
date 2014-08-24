#pragma once

#include "ocl_nfdev.h"
#include "../weak_contexted.h"
#include "../data_array_factory.h"

namespace nf
{
    struct ocl_data_array_factory : weak_contexted<ocl_computation_context>, virtual data_array_factory
    {
        ocl_data_array_factory(const ocl_computation_context_wptr& context);

        data_array_ptr create(idx_t size, float fill) override;
        data_array_ptr create(const float* values, idx_t beginPos, idx_t size) override;
        data_array_ptr create_const(const float* values, idx_t beginPos, idx_t size) override;
    };
}
