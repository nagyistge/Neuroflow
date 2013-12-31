#include "stdafx.h"
#include "ocl_exec.h"
#include "ocl_device_array.h"
#include "ocl_program.h"
#include "ocl_internal_context.h"

USING;

const std::string& ocl_exec::get_kernel_name(idx_t vectorSize)
{
    return get_named_kernel(vectorSize).kernelName;
}

void ocl_exec::execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, idx_t workItemSize)
{
    ensure_kernel(program, kernelName, vectorSize, setupKernel);
    do_execute(program, vectorSize, cl::NullRange, workItemSize > 1 ? cl::NDRange(workItemSize) : cl::NullRange, cl::NullRange);
}

void ocl_exec::execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const ocl_device_array_ptr& extent)
{
    ensure_kernel(program, kernelName, vectorSize, setupKernel);
    do_execute(program, vectorSize, cl::NullRange, cl::NDRange(extent->size()), cl::NullRange);
}

void ocl_exec::execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    ensure_kernel(program, kernelName, vectorSize, setupKernel);
    do_execute(program, vectorSize, cl::NullRange, workItemSizes, localSizes);
}

void ocl_exec::execute(const ocl_program_ptr& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    ensure_kernel(program, kernelName, vectorSize, setupKernel);
    do_execute(program, vectorSize, workItemOffsets, workItemSizes, localSizes);
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

ocl_exec::named_kernel& ocl_exec::get_named_kernel(idx_t vectorSize)
{
    idx_t index = vector_size_to_index(vectorSize);
    kernels.resize(index + 1);
    return kernels[index];
}

void ocl_exec::ensure_kernel(const ocl_program_ptr& program, const std::string& kernelName, idx_t vectorSize, const std::function<void(cl::Kernel&)>& setupKernel)
{
    auto& k = get_named_kernel(vectorSize);
    if (k.kernelName.size() == 0) k.kernelName = kernelName;
    if (k.kernel() == nullptr) k.kernel = program->create_kernel(kernelName);
    setupKernel(k.kernel);
}

void ocl_exec::do_execute(const ocl_program_ptr& program, idx_t vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    auto& k = get_named_kernel(vectorSize);
    if (workItemSizes.dimensions() == 0 || workItemSizes.dimensions() == 1 && workItemSizes[0] == 1)
    {
        program->context()->cl_queue().enqueueTask(k.kernel);
    }
    else
    {
        program->context()->cl_queue().enqueueNDRangeKernel(k.kernel, workItemOffsets, workItemSizes, localSizes);
    }
}