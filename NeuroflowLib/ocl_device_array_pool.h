#pragma once

#include "ocl_nfdev.h"
#include "device_array_pool.h"
#include "ocl_contexted.h"

namespace nf
{
    struct ocl_device_array_pool : ocl_contexted, virtual device_array_pool
    {
        friend struct ocl_device_array;

        ocl_device_array_pool(const ocl_computation_context_wptr& context);

        bool is_allocated() const override;
        device_array_ptr create_array(idx_t size) override;
        device_array2_ptr create_array2(idx_t rowSize, idx_t colSize) override;
        void allocate() override;
        void zero() override;

    private:
        cl::Buffer buffer;
        idx_t endIndex = 0;

        idx_t reserve(idx_t size);
        cl::Buffer create_sub_buffer(idx_t beginOffset, idx_t size);
    };
}