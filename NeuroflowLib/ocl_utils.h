#pragma once

#include "utils.h"

namespace nf
{
    struct ocl_utils : virtual utils
    {
        friend struct ocl_device_array_pool;

        void zero(device_array_ptr& deviceArray) override;

    private:
        void zero(const cl::Buffer buffer, idx_t size);
    };
}