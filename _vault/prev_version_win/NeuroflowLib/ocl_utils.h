#pragma once

#include "utils.h"
#include "ocl_nfdev.h"
#include "weak_contexted.h"
#include "ocl_kernel_name.h"
#include "ocl_exec.h"

namespace nf
{
    struct ocl_utils : weak_contexted<ocl_computation_context>, virtual utils
    {
        friend struct ocl_device_array_pool;
        friend struct ocl_computation_context;

        ocl_utils(const ocl_computation_context_wptr& context);

        void randomize_uniform(const device_array_ptr& deviceArray, float min, float max) override;
        void calculate_mse(supervised_batch& batch, const data_array_ptr& dataArray, idx_t valueIndex) override;
        void zero(const device_array_ptr& deviceArray) override;

    private:
        static ocl_kernel_name addMSEName;
        static ocl_kernel_name divName;
        static ocl_kernel_name zeroFName;

        std::mt19937 generator;
        ocl_program_ptr program;
        ocl_exec addExec, divExec, zeroFExec;
        cl_float2 z2;
        cl_float4 z4;
        cl_float8 z8;
        cl_float16 z16;

        void build();
        void zero(const cl::Buffer& buffer, idx_t size);
        idx_t get_preferred_workgroup_size_mul();
        void add_mse(const ocl_data_array_ptr& desiredValues, const ocl_data_array_ptr& currentValues, const ocl_data_array_ptr& mseValues, idx_t mseValueIndex);
        void div(const ocl_data_array_ptr& values, idx_t valueIndex, float byValue);
    };
}