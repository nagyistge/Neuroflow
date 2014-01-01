#pragma once

#include "ocl_nfdev.h"
#include "ocl_contexted.h"

namespace nf
{
    struct ocl_exec : ocl_contexted
    {
        struct named_kernel
        {
            cl::Kernel kernel;
            std::string kernelName;
        };

        ocl_exec(const ocl_computation_context_wptr& context);

        const std::string& get_kernel_name(idx_t vectorSize);
        void execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, idx_t workItemSize = 1);
        void execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const ocl_device_array_ptr& extent);
        void execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
        void execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);

    private:
        std::vector<named_kernel> kernels;

        inline idx_t vector_size_to_index(idx_t vectorSize) const;
        named_kernel& get_named_kernel(idx_t vectorSize);
        void ensure_kernel(const ocl_program_ptr& program, const std::string& kernelName, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel);
        void do_execute(const ocl_program_ptr& program, idx_t vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
    };
}