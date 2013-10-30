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
    auto& ctx = program->GetIntCtx();
    unsigned preferredSizeMul = (ctx->IsCPU() ? 32 : ctx->GetPreferredWorkgroupSizeMul());

    if (workItemSizes.dimensions() == 0 || workItemSizes.dimensions() == 1 && workItemSizes[0] == 1)
    {
        ctx->GetQueue().enqueueTask(data.kernel);
    }
    else if (!noSizeOpt && localSizes.dimensions() == 0 && workItemSizes.dimensions() == 1 && workItemSizes[0] > preferredSizeMul)
    {
        unsigned size = workItemSizes[0];
        unsigned rem = size % preferredSizeMul;
        size -= rem;

        ctx->GetQueue().enqueueNDRangeKernel(
            data.kernel,
            workItemOffsets,
            NDRange(size),
            NullRange);
    }
    else 
    {
        ctx->GetQueue().enqueueNDRangeKernel(
            data.kernel,
            workItemOffsets,
            workItemSizes,
            localSizes);
    }
}