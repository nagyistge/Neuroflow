#pragma once

#include "ocl_nfdev.h"
#include "../device_array_management.h"
#include "../weak_contexted.h"

namespace nf
{
    struct ocl_device_array_management : weak_contexted<ocl_computation_context>, virtual device_array_management
    {
        friend struct ocl_device_array_pool;
        friend struct ocl_data_array_factory;

        ocl_device_array_management(const ocl_computation_context_wptr& context);

        device_array_ptr create_array(bool copyOptimized, idx_t size) override;
        device_array2_ptr create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize) override;
        void copy(const device_array_ptr& from, idx_t fromIndex, const device_array_ptr& to, idx_t toIndex, idx_t size) override;
        device_array_pool_ptr create_pool(bool copyOptimized) override;

    private:

        cl::Buffer create_buffer(cl_mem_flags flags, idx_t sizeInBytes, float fill = 0.0f);
        cl::Buffer create_buffer(cl_mem_flags flags, const float* from, idx_t sizeInBytes);
    };
}
