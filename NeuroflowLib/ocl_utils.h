#pragma once

#include "utils.h"

namespace nf
{
    struct ocl_utils : virtual utils
    {
        friend struct ocl_device_array_pool;

        void randomize_uniform(device_array_ptr deviceArray, float min, float max) override;
        void calculate_mse(supervised_batch& batch, data_array_ptr dataArray, idx_t valueIndex) const override;
        void zero(device_array_ptr deviceArray) override;

    private:
        void zero(const cl::Buffer buffer, idx_t size);
    };
}