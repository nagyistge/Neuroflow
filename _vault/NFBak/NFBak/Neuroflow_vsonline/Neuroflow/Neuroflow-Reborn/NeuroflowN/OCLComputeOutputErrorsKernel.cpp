#include "stdafx.h"
#include "OCLComputeOutputErrorsKernel.h"
#include "OCLProgramBuilder.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLKernelToExecute.h"

using namespace NeuroflowN;
using namespace std;
using namespace cl;

OCLVectorKernelName OCLComputeOutputErrorsKernel::ComputeErrors_Output_Sigmoid = OCLVectorKernelName("ComputeErrors_Output_Sigmoid");
OCLVectorKernelName OCLComputeOutputErrorsKernel::ComputeErrors_Output_Linear = OCLVectorKernelName("ComputeErrors_Output_Linear");

void OCLComputeOutputErrorsKernel::Build(OCLProgramBuilder& program)
{
    // Compute Errors:
    DEFINE_OCL_PROGRAM(program,

    /*
    ComputeErrors Output Sigmoid
    */

    __kernel void ComputeErrors_Output_Sigmoid$(
    __global float$* errors,
    __global float$* desiredOutputs,
    __global float$* outputs,
    float alpha)
    {
        int idx = get_global_id(0);
        errors[idx] = (desiredOutputs[idx] - outputs[idx]) * SigmoidD$(outputs[idx], alpha);
    }

    /*
    ComputeErrors Output Linear
    */

    __kernel void ComputeErrors_Output_Linear$(
        __global float$* errors,
        __global float$* desiredOutputs,
        __global float$* outputs,
        float alpha)
    {
        int idx = get_global_id(0);
        errors[idx] = (desiredOutputs[idx] - outputs[idx]) * alpha;
    }

    );
}

void OCLComputeOutputErrorsKernel::Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha)
{
    auto exec = (OCLKernelToExecute*) state;
    auto& outputs = ctx->ToBuffer1(pOutputs);
    auto& errors = ctx->ToBuffer1(pErrors);
    auto& desiredOutputs = ctx->ToBuffer1(pDesiredOutputs);

    unsigned vectorSize = GetVectorSize(cref(outputs));

    auto init = [=](Kernel& kernel)
    {
        int aidx = 0;
        kernel.setArg(aidx++, errors.GetCLBuffer());
        kernel.setArg(aidx++, desiredOutputs.GetCLBuffer());
        kernel.setArg(aidx++, outputs.GetCLBuffer());
        kernel.setArg(aidx++, alpha);
    };

    if (function == ActivationFunction::Sigmoid)
    {
        exec->Execute(
            ctx,
            ComputeErrors_Output_Sigmoid(vectorSize),
            vectorSize,
            init,
            errors.GetSize() / vectorSize);
    }
    else
    {
        exec->Execute(
            ctx,
            ComputeErrors_Output_Linear(vectorSize),
            vectorSize,
            init,
            errors.GetSize() / vectorSize);
    }
}
