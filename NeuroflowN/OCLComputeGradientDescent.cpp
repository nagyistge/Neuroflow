#include "stdafx.h"
#include "OCLComputeGradientDescent.h"
#include "OCLProgram.h"
#include "OCLVault.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLVectorKernelName OCLComputeGradientDescent::GD_Online_Smooth = OCLVectorKernelName("GD_Online_Smooth");
OCLVectorKernelName OCLComputeGradientDescent::GD_Online = OCLVectorKernelName("GD_Online");
OCLVectorKernelName OCLComputeGradientDescent::GD_Offline_Smooth = OCLVectorKernelName("GD_Offline_Smooth");
OCLVectorKernelName OCLComputeGradientDescent::GD_Offline = OCLVectorKernelName("GD_Offline");

void OCLComputeGradientDescent::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientDescentPrg");
    program->Using(vault->GetCommonCode());

    ADD_OCL_CODE(program,

    // Online
    __kernel void GD_Online_Smooth$(
    __global float$* weights,
    __global float$* gradients,
    __global float$* lastUpdates,
    float rate,
    float momentum)
    {
        int idx = get_global_id(0);
        float$ update = gradients[idx] * rate;
        float$ lastUpdate = lastUpdates[idx];
        update = lastUpdate * momentum + (update * (1.0f - momentum));
        weights[idx] += update;
        lastUpdates[idx] = update;
        }

    __kernel void GD_Online$(
        __global float$* weights,
        __global float$* gradients,
        __global float$* lastUpdates,
        float rate,
        float momentum)
    {
        int idx = get_global_id(0);
        float$ update = gradients[idx] * rate;
        float$ lastUpdate = lastUpdates[idx];
        update = lastUpdate * momentum + update;
        weights[idx] += update;
        lastUpdates[idx] = update;
    }

    // Offline
    __kernel void GD_Offline_Smooth$(
        __global float$* weights,
        __global float$* gradientSums,
        __global float$* lastUpdates,
        float iterationCount,
        float rate,
        float momentum)
    {
        int idx = get_global_id(0);
        float$ update = (gradientSums[idx] / iterationCount) * rate;
        float$ lastUpdate = lastUpdates[idx];
        update = lastUpdate * momentum + (update * (1.0f - momentum));
        weights[idx] += update;
        lastUpdates[idx] = update;
    }

    __kernel void GD_Offline$(
        __global float$* weights,
        __global float$* gradientSums,
        __global float$* lastUpdates,
        float iterationCount,
        float rate,
        float momentum)
    {
        int idx = get_global_id(0);
        float$ update = (gradientSums[idx] / iterationCount) * rate;
        float$ lastUpdate = lastUpdates[idx];
        update = lastUpdate * momentum + update;
        weights[idx] += update;
        lastUpdates[idx] = update;
    }

    );
}

void OCLComputeGradientDescent::UpdateWeightsOnline(
    OCLKernelToExecute& exec,
    const OCLBuffer1& lastUpdates,
    const OCLBuffer1& weights,
    const OCLBuffer1& gradients,
    float rate,
    float momentum,
    bool smoothing)
{
    try
    {
        unsigned vectorSize = GetVectorSize(cref(weights));
        if (smoothing)
        {
            exec.Execute(
                program,
                GD_Online_Smooth(vectorSize),
                vectorSize,
                [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, weights.GetCLBuffer());
                    kernel.setArg(aidx++, gradients.GetCLBuffer());
                    kernel.setArg(aidx++, lastUpdates.GetCLBuffer());
                    kernel.setArg(aidx++, rate);
                    kernel.setArg(aidx++, momentum);
                },
                weights.GetSize() / vectorSize);
        }
        else
        {
            exec.Execute(
                program,
                GD_Online(vectorSize),
                vectorSize,
                [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, weights.GetCLBuffer());
                    kernel.setArg(aidx++, gradients.GetCLBuffer());
                    kernel.setArg(aidx++, lastUpdates.GetCLBuffer());
                    kernel.setArg(aidx++, rate);
                    kernel.setArg(aidx++, momentum);
                },
                weights.GetSize() / vectorSize);
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeGradientDescent::UpdateWeightsOffline(
    OCLKernelToExecute& exec,
    const OCLBuffer1& lastUpdates,
    const OCLBuffer1& weights,
    const OCLBuffer1& gradientSums,
    int iterationCount,
    float rate,
    float momentum,
    bool smoothing)
{
    float fitc = (float)iterationCount;
    try
    {
        unsigned vectorSize = GetVectorSize(cref(weights));
        if (smoothing)
        {
            exec.Execute(
                program,
                GD_Offline_Smooth(vectorSize),
                vectorSize,
                [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, weights.GetCLBuffer());
                    kernel.setArg(aidx++, gradientSums.GetCLBuffer());
                    kernel.setArg(aidx++, lastUpdates.GetCLBuffer());
                    kernel.setArg(aidx++, fitc);
                    kernel.setArg(aidx++, rate);
                    kernel.setArg(aidx++, momentum);
                },
                weights.GetSize() / vectorSize);
        }
        else
        {
            exec.Execute(
                program,
                GD_Offline(vectorSize),
                vectorSize,
                [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, weights.GetCLBuffer());
                    kernel.setArg(aidx++, gradientSums.GetCLBuffer());
                    kernel.setArg(aidx++, lastUpdates.GetCLBuffer());
                    kernel.setArg(aidx++, fitc);
                    kernel.setArg(aidx++, rate);
                    kernel.setArg(aidx++, momentum);
                },
                weights.GetSize() / vectorSize);
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}