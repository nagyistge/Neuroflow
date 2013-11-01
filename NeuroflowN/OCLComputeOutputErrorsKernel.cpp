#include "stdafx.h"
#include "OCLComputeOutputErrorsKernel.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLComputationState.h"
#include "OCLVault.h"

using namespace NeuroflowN;
using namespace std;
using namespace cl;

OCLComputeOutputErrorsKernel::OCLComputeOutputErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
    OCLVersionableKernelBase(ctx, "ComputeOutputErrors", { SigmoidCOKV, LinearCOKV }, 1, false)
{
    Build(vault);
}

void OCLComputeOutputErrorsKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeOutputErrorsPrg");
    program->Using(vault->GetCommonCode());
    program->Using(vault->GetAFCode());

    auto getCode = [=](const std::string& name, const char* calcCode)
    {
        stringstream code;
        code << "__kernel void " << name << "$(";
        code << "__global float$* errors,";
        code << "__global float$* desiredOutputs,";
        code << "__global float$* outputs,";
        code << "float alpha)";
        code << "{";
        code << "int idx = get_global_id(0);";
        code << "errors[idx] = (desiredOutputs[idx] - outputs[idx]) * " << calcCode << ";";
        code << "}";

        return code.str();
    };

    program->AddCode(getCode(GetNames().GetVersion(SigmoidCOKV)->GetName(), "SigmoidD$(outputs[idx], alpha)"));
    program->AddCode(getCode(GetNames().GetVersion(LinearCOKV)->GetName(), "alpha"));
}

void OCLComputeOutputErrorsKernel::Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha)
{
    auto& exec = ((OCLComputationState*)state)->GetExec(0);
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
        exec.Execute(
            program,
            (*GetNames().GetVersion(SigmoidCOKV))(vectorSize),
            vectorSize,
            init,
            errors.GetSize() / vectorSize);
    }
    else
    {
        exec.Execute(
            program,
            (*GetNames().GetVersion(LinearCOKV))(vectorSize),
            vectorSize,
            init,
            errors.GetSize() / vectorSize);
    }
}
