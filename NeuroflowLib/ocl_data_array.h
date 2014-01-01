#pragma once

#include "cpp_nfdev.h"
#include "ocl_device_array.h"
#include "data_array.h"
#include "ocl_contexted.h"

namespace nf
{
    struct ocl_data_array : ocl_device_array, ocl_contexted, virtual data_array
    {
        ocl_data_array(const ocl_computation_context_wptr& context, const cl::Buffer& buffer, bool isConst);

        bool is_const() const;
        concurrency::task<void> read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex);
        concurrency::task<void> write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex);

    private:
        bool isConst;

        inline void verify_if_accessible() const;
    };
}