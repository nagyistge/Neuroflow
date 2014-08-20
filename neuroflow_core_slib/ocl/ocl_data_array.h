#pragma once

#include "ocl_nfdev.h"
#include "ocl_device_array.h"
#include "../data_array.h"
#include "../contexted.h"

namespace nf
{
    struct ocl_data_array : ocl_device_array, contexted<ocl_computation_context>, virtual data_array
    {
        ocl_data_array(const ocl_computation_context_ptr& context, const cl::Buffer& buffer, bool isConst);

        bool is_const() const;
        boost::shared_future<void> read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex);
        boost::shared_future<void> write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex);

    private:
        bool isConst;

        inline void verify_if_accessible() const;
    };
}
