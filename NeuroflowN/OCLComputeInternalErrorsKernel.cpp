#include "stdafx.h"
#include "OCLComputeInternalErrorsKernel.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLComputationState.h"
#include "OCL.h"
#include "OCLVault.h"

using namespace std;
using namespace NeuroflowN;
using namespace cl;

OCLComputeInternalErrorsKernel::OCLComputeInternalErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
OCLVersionableKernelBase(ctx, "ComputeInternalErrors", { AKVSigmoid, AKVLinear }, ctx->GetMaxConnectionCount())
{
    Build(vault);
};

void OCLComputeInternalErrorsKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeInternalErrorsPrg");
    if (ctx->IsCPU()) program->Using(vault->GetNetCode()); else program->Using(vault->GetCommonCode());
    program->Using(vault->GetAFCode());

    if (ctx->IsCPU())
    {
        ADD_OCL_CODE(program,

        float$ ComputeErrors_LowerErrorSum$(__global float* lowerErrors, int lowerErrorsSize, __global float$* lowerWeights, int idx, int currentOutputsSize)
        {
            float$ sum = 0.0f;
            for (int x = 0; x < lowerErrorsSize; x++) sum += lowerErrors[x] * Get2$(lowerWeights, idx, x, currentOutputsSize);
            return sum;
        }

        );
    }

    for (unsigned size = 1; size <= ctx->GetMaxConnectionCount(); size++)
    {
        if (ctx->IsCPU())
        {
            auto cpuCode = CreateCPUKernelCode(size);
            program->AddCode(cpuCode);
        }
        else
        {
            auto gpuCode = CreateGPUKernelCode(size);
            program->AddCode(gpuCode);
        }
    }
}

std::string OCLComputeInternalErrorsKernel::CreateCPUKernelCode(unsigned size)
{
    auto& names = GetCPUNames(size);

    auto factory = [size](const string& name, const char* calcCode, bool hasOutputVector)
    {
        stringstream code;
        code << "__kernel void " << name << "$(";
        code << "__global float$* errors,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float* lowerErrors" << i << ",";
            code << "int lowerErrorsSize" << i << ",";
        }
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* lowerWeights" << i << ",";
        }
        if (hasOutputVector) code << "__global float$* outputs,";
        code << "int outputsSize,";
        code << "float alpha)";
        code << "{";
        code << "int idx = get_global_id(0);";
        code << "float$ sum =";
        for (unsigned i = 0; i < size; i++)
        {
            code << "ComputeErrors_LowerErrorSum$(lowerErrors" << i << ", lowerErrorsSize" << i << ", lowerWeights" << i << ", idx, outputsSize)";
            if (i != size - 1) code << "+";
        }
        code << ";";
        code << "errors[idx] = sum * " << calcCode << ";";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    code << factory(names.GetVersion(AKVSigmoid).GetName(), "SigmoidD$(outputs[idx], alpha)", true);
    code << factory(names.GetVersion(AKVLinear).GetName(), "alpha", false);

    return code.str();
}

std::string OCLComputeInternalErrorsKernel::CreateGPUKernelCode(unsigned size)
{
    auto& names = GetGPUNames(size);

    auto factory = [size](const string& name, const char* calcCode, bool hasOutputVector)
    {
        stringstream code;
        code << "__kernel void " << name << "$(";
        code << "__global float$* errors,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float* lowerErrors" << i << ",";
            code << "int lowerErrorsSize" << i << ",";
        }
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* lowerWeights" << i << ",";
        }
        if (hasOutputVector) code << "__global float$* outputs,";
        code << "int outputsSize,";
        code << "float alpha)";
        code << "{";
        code << "__local int$ sum; int oidx = get_group_id(0); int leidx = get_local_id(0); int lsize = get_local_size(0); if (leidx == 0) sum = 0;";
        code << "barrier(CLK_LOCAL_MEM_FENCE);";
        for (unsigned i = 0; i < size; i++)
        {
            code << 
                "for (int cleidx = leidx, i = 0; cleidx < lowerErrorsSize" << i << "; cleidx += lsize, i++)" <<
                "{" <<
                "int$ v = convert_int$_rte(lowerErrors" << i << "[cleidx] * lowerWeights" << i << "[GetIndex2(oidx, cleidx, outputsSize)] * D);" <<
                "AtomAdd$(&sum, v);" <<
                "}";
        }
        code << "barrier(CLK_LOCAL_MEM_FENCE);";
        code << "if (leidx == 0) errors[oidx] = (convert_float$(sum) / D) * " << calcCode << ";";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    code << factory(names.GetVersion(AKVSigmoid).GetName(), "SigmoidD$(outputs[oidx], alpha)", true);
    code << factory(names.GetVersion(AKVLinear).GetName(), "alpha", false);

    return code.str();
}

void OCLComputeInternalErrorsKernel::Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha)
{
    using namespace std::placeholders;

    unsigned size = lowerErrors->size();
    assert(lowerWeights->size() == size);

    auto& exec = ((OCLComputationState*)state)->GetExec(0);
    auto& outputs = ctx->ToBuffer1(pOutputs);
    auto& errors = ctx->ToBuffer1(pErrors);

    unsigned vectorSize = GetVectorSize(cref(outputs));

    auto init = [=](Kernel& kernel, bool hasOutputVector)
    {
        int aidx = 0;
        kernel.setArg(aidx++, errors.GetCLBuffer());
        for (unsigned i = 0; i < size; i++)
        {
            auto& lowerErrorsI = ctx->ToBuffer1((*lowerErrors)[i]);
            kernel.setArg(aidx++, lowerErrorsI.GetCLBuffer());
            kernel.setArg(aidx++, lowerErrorsI.GetSize());
        }
        for (unsigned i = 0; i < size; i++)
        {
            auto& lowerWeightsI = ctx->ToBuffer1((*lowerWeights)[i]);
            kernel.setArg(aidx++, lowerWeightsI.GetCLBuffer());
        }
        if (hasOutputVector) kernel.setArg(aidx++, outputs.GetCLBuffer());
        kernel.setArg(aidx++, outputs.GetSize() / vectorSize);
        kernel.setArg(aidx++, alpha);
    };

    auto initSig = bind(init, _1, true);
    auto initLin = bind(init, _1, false);

    if (ctx->IsCPU())
    {
        if (function == ActivationFunction::Sigmoid)
        {
            exec.Execute(
                program,
                GetCPUNames(size).GetVersion(AKVSigmoid)(vectorSize),
                vectorSize,
                initSig,
                errors.GetSize() / vectorSize);
        }
        else
        {
            exec.Execute(
                program,
                GetCPUNames(size).GetVersion(AKVLinear)(vectorSize),
                vectorSize,
                initLin,
                errors.GetSize() / vectorSize);
        }
    }
    else
    {
        auto sizes = GetIOReduceSizesOutput(lowerErrors, outputs, vectorSize);

        if (function == ActivationFunction::Sigmoid)
        {
            exec.Execute(
                program,
                GetGPUNames(size).GetVersion(AKVSigmoid)(vectorSize),
                vectorSize,
                initSig,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
        else
        {
            exec.Execute(
                program,
                GetGPUNames(size).GetVersion(AKVLinear)(vectorSize),
                vectorSize,
                initLin,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
    }
}
