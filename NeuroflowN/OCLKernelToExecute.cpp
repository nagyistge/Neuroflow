#include "stdafx.h"
#include "OCLKernelToExecute.h"
#include "OCLError.h"

using namespace NeuroflowN;
using namespace cl;
using namespace std;

void OCLKernelToExecute::EnsureKernel(const OCLProgramSPtrT& program, const std::string& kernelName, unsigned vectorSize, const std::function<void(cl::Kernel&)>& setupKernel)
{
    auto& data = GetData(vectorSize);
    if (data.kernelName.size() == 0) data.kernelName = kernelName;
    if (data.kernel() == nullptr) data.kernel = program->CreateKernel(kernelName);
    setupKernel(data.kernel);
}

void OCLKernelToExecute::DoExecute(const OCLProgramSPtrT& program, unsigned vectorSize, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    auto& data = GetData(vectorSize);
    if (workItemSizes.dimensions() == 0)
    {
        program->GetIntCtx()->GetQueue().enqueueTask(data.kernel);
    }
    else if (localSizes.dimensions() != 0) // AKA: not NullRange
    {
        EnqueueNDRangeKernel(program, data.kernel, workItemOffsets, workItemSizes, localSizes);
    }
    else
    {
        EnqueueNDRangeKernel(program, data.kernel, workItemOffsets, workItemSizes, localSizes); // TODO: Optimize this path
    }
}

void OCLKernelToExecute::EnqueueNDRangeKernel(const OCLProgramSPtrT& program, const cl::Kernel& kernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    program->GetIntCtx()->GetQueue().enqueueNDRangeKernel(
        kernel,
        workItemOffsets,
        workItemSizes,
        localSizes);
}