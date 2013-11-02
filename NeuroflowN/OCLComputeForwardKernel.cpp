#include "stdafx.h"
#include "OCLComputeForwardKernel.h"
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

OCLComputeForwardKernel::OCLComputeForwardKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
    OCLVersionableKernelBase(ctx, "ComputeForward", { SigmoidAKV, LinearAKV, RTLRAKV }, ctx->GetMaxConnectionCount())
{
    Build(vault);
};

void OCLComputeForwardKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeForwardPrg");
    if (ctx->IsCPU()) program->Using(vault->GetNetCode()); else program->Using(vault->GetCommonCode());
    program->Using(vault->GetAFCode());

    if (ctx->IsCPU())
    {
        ADD_OCL_CODE(program,

        float ComputeForward_Sum$(global float$* inputs, int inputsSize, global float$* weights, int idx)
        {
            float$ sum = 0.0f;
            for (int x = 0; x < inputsSize; x++) sum += inputs[x] * Get2$(weights, x, idx, inputsSize);
            return SumComponents$(sum);
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

std::string OCLComputeForwardKernel::CreateCPUKernelCode(unsigned size)
{
    auto& names = GetCPUNames(size);

    auto factory = [size](const string& name, const char* calcCode, const char* calcNVDCode)
    {
        stringstream code;
        code << "kernel void " << name << "$(";
        for (unsigned i = 0; i < size; i++)
        {
            code << "global float$* inputs" << i << ",";
            code << "int inputsSize" << i << ",";
        }
        code << "global float* biases,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "global float$* weights" << i << ",";
        }
        code << "global float* outputs";
        code << ",float alpha";
        if (calcNVDCode != null) code << ",global float* netValueDerivates";
        code <<")";
        code << "{";
        code << "int idx = get_global_id(0);";
        code << "float sum = biases[idx]";
        for (unsigned i = 0; i < size; i++)
        {
            code << " + ";
            code << "ComputeForward_Sum$(inputs" << i << ", inputsSize" << i << ", weights" << i << ", idx)";
        }
        code << ";";
        if (calcNVDCode != null) code << "netValueDerivates[idx] = " << calcNVDCode << ";";
        code << "outputs[idx] = " << calcCode << ";";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    auto sig = "Sigmoid(sum, alpha)";
    auto lin = "fmax(fmin(sum * alpha, alpha), -alpha)";
    code << factory(names.GetVersion(SigmoidAKV)->GetName(), sig, null);
    code << factory(names.GetVersion(LinearAKV)->GetName(), lin, null);
    code << factory(names.GetVersion(SigmoidAKV | RTLRAKV)->GetName(), sig, "SigmoidD(sum, alpha)");
    code << factory(names.GetVersion(LinearAKV | RTLRAKV)->GetName(), lin, "alpha");

    return code.str();
}

std::string OCLComputeForwardKernel::CreateGPUKernelCode(unsigned size)
{
    auto& names = GetGPUNames(size);

    auto factory = [size](const string& name, const char* calcCode, const char* calcNVDCode)
    {
        stringstream code;
        code << "kernel void " << name << "$(";
        for (unsigned i = 0; i < size; i++)
        {
            code << "global float$* inputs" << i << ",";
            code << "int inputsSize" << i << ",";
        }
        code << "global float* biases,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "global float$* weights" << i << ",";
        }
        code << "global float* outputs";
        code << ",float alpha";
        if (calcNVDCode != null) code << ",global float* netValueDerivates";
        code << ")";
        code << "{";
        code << "local int$ sum; int oidx = get_group_id(0); int iidx = get_local_id(0); int lsize = get_local_size(0); if (iidx == 0) sum = 0; barrier(CLK_LOCAL_MEM_FENCE);";
        for (unsigned i = 0; i < size; i++)
        {
            code << "for (int ciidx = iidx; ciidx < inputsSize" << i << "; ciidx += lsize)";
            code << "{";
            code << "int$ v = convert_int$_rte(inputs" << i << "[ciidx] * weights" << i << "[GetIndex2(ciidx, oidx, inputsSize" << i << ")] * D);";
            code << "AtomAdd$(&sum, v);";
            code << "}";
        }
        code << "barrier(CLK_LOCAL_MEM_FENCE);";
        code << "if (iidx == 0)";
        code << "{";
        code << "float$ sumf = convert_float$(sum) / D;";
        code << "float sumf1 = biases[oidx] + SumComponents$(sumf);";
        if (calcNVDCode != null) code << "netValueDerivates[oidx] = " << calcNVDCode << ";";
        code << "outputs[oidx] = " << calcCode << ";";
        code << "}";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    auto sig = "Sigmoid(sumf1, alpha)";
    auto lin = "fmax(fmin(sumf1 * alpha, alpha), -alpha)";
    code << factory(names.GetVersion(SigmoidAKV)->GetName(), sig, null);
    code << factory(names.GetVersion(LinearAKV)->GetName(), lin, null);
    code << factory(names.GetVersion(SigmoidAKV | RTLRAKV)->GetName(), sig, "SigmoidD(sumf1, alpha)");
    code << factory(names.GetVersion(LinearAKV | RTLRAKV)->GetName(), lin, "alpha");

    return code.str();
}

void OCLComputeForwardKernel::Exec(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, IDeviceArray* pNetValueDerivates, ActivationFunction function, float alpha)
{
    unsigned size = (unsigned)inputs->size();
    assert(size == weights->size());

    auto& exec = ((OCLComputationState*)state)->GetExec(0);
    auto& biases = ctx->ToBuffer1(pBiases);
    auto& outputs = ctx->ToBuffer1(pOutputs);

    unsigned vectorSize = CalculateVectorSize(inputs);

    auto init = [=](Kernel& kernel)
    {
        int aidx = 0;
        for (unsigned i = 0; i < size; i++)
        {
            auto& inputsI = ctx->ToBuffer1((*inputs)[i]());
            kernel.setArg(aidx++, inputsI.GetCLBuffer());
            kernel.setArg(aidx++, inputsI.GetSize() / vectorSize);
        }
        kernel.setArg(aidx++, biases.GetCLBuffer());
        for (unsigned i = 0; i < size; i++)
        {
            auto& weightsI = ctx->ToBuffer1((*weights)[i]);
            kernel.setArg(aidx++, weightsI.GetCLBuffer());
        }
        kernel.setArg(aidx++, outputs.GetCLBuffer());
        kernel.setArg(aidx++, alpha);
        if (pNetValueDerivates != null) kernel.setArg(aidx++, ctx->ToBuffer1(pNetValueDerivates).GetCLBuffer());
    };

    if (ctx->IsCPU())
    {
        if (function == ActivationFunction::Sigmoid)
        {
            exec.Execute(
                program,
                (*GetCPUNames(size).GetVersion(pNetValueDerivates != null ? (SigmoidAKV | RTLRAKV) : SigmoidAKV))(vectorSize),
                vectorSize,
                init,
                outputs.GetSize());
        }
        else
        {
            exec.Execute(
                program,
                (*GetCPUNames(size).GetVersion(pNetValueDerivates != null ? (LinearAKV | RTLRAKV) : LinearAKV))(vectorSize),
                vectorSize,
                init,
                outputs.GetSize());
        }
    }
    else
    {
        auto sizes = GetIOReduceSizesInput(inputs, outputs, vectorSize);

        if (function == ActivationFunction::Sigmoid)
        {
            exec.Execute(
                program,
                (*GetGPUNames(size).GetVersion(pNetValueDerivates != null ? (SigmoidAKV | RTLRAKV) : SigmoidAKV))(vectorSize),
                vectorSize,
                init,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
        else
        {
            exec.Execute(
                program,
                (*GetGPUNames(size).GetVersion(pNetValueDerivates != null ? (LinearAKV | RTLRAKV) : LinearAKV))(vectorSize),
                vectorSize,
                init,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
    }
}
