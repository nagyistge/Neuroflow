#pragma once

#include "ocl_nf.h"
#include "ocl_contexted.h"
#include "data_array_factory.h"

namespace nf
{
    struct ocl_data_array_factory : virtual data_array_factory
    {
        ocl_data_array_factory(const ocl_device_array_management_ptr& deviceArrayMan);

        data_array_ptr create(idx_t size, float fill) override;
        data_array_ptr create(float* values, idx_t beginPos, idx_t size) override;
        data_array_ptr create_const(float* values, idx_t beginPos, idx_t size) override;

    private:
        ocl_device_array_management_ptr deviceArrayMan;
    };
}