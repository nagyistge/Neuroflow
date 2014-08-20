#pragma once

#include "ocl_nfdev.h"
#include "../weak_contexted.h"
#include "ocl_kernel_name.h"

namespace nf
{
    struct ocl_exec : weak_contexted<ocl_computation_context>
    {
        ocl_exec(const ocl_computation_context_wptr& context, const ocl_kernel_name& kernelName);

        const ocl_kernel_name& kernel_name() const;
        void execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, idx_t workItemSize = 1);
        void execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const ocl_device_array_ptr& extent);
        void execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
        void execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);

    private:
        ocl_kernel_name kernelName;
        std::array<cl::Kernel, 5> kernels;

        inline idx_t vector_size_to_index(idx_t vectorSize) const;
        inline cl::Kernel& get_kernel(idx_t vectorSize);
        void ensure_kernel(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel);
        void do_execute(idx_t vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
    };
}
