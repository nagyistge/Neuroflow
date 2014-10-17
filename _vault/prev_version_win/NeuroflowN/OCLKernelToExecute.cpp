#include "stdafx.h"
#include "OCLKernelToExecute.h"
#include "OCLError.h"
#include "OCLOutOfOrderQueue.h"

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
    if (workItemSizes.dimensions() == 0 || workItemSizes.dimensions() == 1 && workItemSizes[0] == 1)
    {
        if (isOutOfOrder)
        {
            program->GetIntCtx()->GetOutOfOrderQueue()->EnqueueTask(data.kernel);
        }
        else
        {
            program->GetIntCtx()->GetQueue().enqueueTask(data.kernel);
        }
    }
    else 
    {
        if (isOutOfOrder)
        {
            program->GetIntCtx()->GetOutOfOrderQueue()->EnqueueNDRangeKernel(
                data.kernel,
                workItemOffsets,
                workItemSizes,
                localSizes);
        }
        else
        {
            program->GetIntCtx()->GetQueue().enqueueNDRangeKernel(
                data.kernel,
                workItemOffsets,
                workItemSizes,
                localSizes);
        }
    }
}