#include "stdafx.h"
#include "ocl_exec.h"
#include "ocl_device_array.h"
#include "ocl_program.h"
#include "ocl_computation_context.h"

USING

ocl_exec::ocl_exec(const ocl_computation_context_wptr& context, const ocl_kernel_name& kernelName) :
weak_contexted(context),
kernelName(kernelName)
{
}

const ocl_kernel_name& ocl_exec::kernel_name() const
{
    return kernelName;
}

void ocl_exec::execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, idx_t workItemSize)
{
    ensure_kernel(program, vectorSize, setupKernel);
    do_execute(vectorSize, cl::NullRange, cl::NDRange(workItemSize), cl::NullRange);
}

void ocl_exec::execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const ocl_device_array_ptr& extent)
{
    ensure_kernel(program, vectorSize, setupKernel);
    do_execute(vectorSize, cl::NullRange, cl::NDRange(extent->size()), cl::NullRange);
}

void ocl_exec::execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    ensure_kernel(program, vectorSize, setupKernel);
    do_execute(vectorSize, cl::NullRange, workItemSizes, localSizes);
}

void ocl_exec::execute(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    ensure_kernel(program, vectorSize, setupKernel);
    do_execute(vectorSize, workItemOffsets, workItemSizes, localSizes);
}

idx_t ocl_exec::vector_size_to_index(idx_t vectorSize) const
{
    switch (vectorSize)
    {
        case 1:
            return 0;
        case 2:
            return 1;
        case 4:
            return 2;
        case 8:
            return 3;
        case 16:
            return 4;
        default:
            throw_invalid_argument("Ivalid vectorSize argument!");
    }
}

Kernel& ocl_exec::get_kernel(idx_t vectorSize)
{
    return kernels[vector_size_to_index(vectorSize)];
}

void ocl_exec::ensure_kernel(const ocl_program_ptr& program, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel)
{
    auto& k = get_kernel(vectorSize);
    if (k() == nullptr)
    {
        k = program->create_kernel(kernelName(vectorSize));
    }
    setupKernel(k);
}

void ocl_exec::do_execute(idx_t vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    auto ctx = lock_context();
    auto& k = get_kernel(vectorSize);
    ctx->cl_queue().enqueueNDRangeKernel(k, workItemOffsets, workItemSizes, localSizes);
}