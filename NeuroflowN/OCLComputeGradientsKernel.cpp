#include "stdafx.h"
#include "OCLComputeGradientsKernel.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLComputationState.h"
#include "OCLVault.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLComputeGradientsKernel::OCLComputeGradientsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
    OCLVersionableKernelBase(ctx, "ComputeGradients", { FFOnlineCGKV, FFOfflineCGKV, FFOnlineOfflineCGKV, BPTTPhase1CGKV, BPTTPhase2CGKV, BPTTPhase2OfflineCGKV })
{
    Build(vault);
};

ComputeGradientsKernelVersion OCLComputeGradientsKernel::GradientComputationFlagsToVersion(GradientComputationFlags flags)
{
    if (flags & FF)
    {
        if (flags & Online)
        {
            if (flags & Offline)
            {
                return FFOnlineOfflineCGKV;
            }
            else
            {
                // Online && !Offline
                return FFOnlineCGKV;
            }
        }
        else
        {
            // !Online
            if (flags & Offline)
            {
                return FFOfflineCGKV;
            }
        }
    }
    else if (flags & BPTTPhase1)
    {
        return BPTTPhase1CGKV;
    }
    else if (flags & BPTTPhase2)
    {
        if (flags & Offline)
        {
            return BPTTPhase2OfflineCGKV;
        }
        else
        {
            return BPTTPhase2CGKV;
        }
    }

    ThrowUnknownFlagsEx(flags);

    throw_logic_error("This ain't gonna happen.");
}

void OCLComputeGradientsKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsPrg");
    if (ctx->IsCPU())
    {
        program->Using(vault->GetNetCode());
        ADD_OCL_CODE(program,

        inline void ComputeGradients_SetGradients$(global float$* inputs, int inputsSize, global float$* gradients, global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++) Set2$(gradients, x, idx, inputsSize, inputs[x] * errors[idx]);
        }

        inline void ComputeGradients_AddGradients$(global float$* inputs, int inputsSize, global float$* gradientSums, global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++) Add2$(gradientSums, x, idx, inputsSize, inputs[x] * errors[idx]);
        }

        inline void ComputeGradients_SetAddGradients$(global float$* inputs, int inputsSize, global float$* gradients, global float$* gradientSums, global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++)
            {
                float$ value = inputs[x] * errors[idx];
                SetAdd2$(gradients, gradientSums, x, idx, inputsSize, value);
            }
        }

        inline void ComputeGradients_AddDivGradients$(global float$* inputs, int inputsSize, global float$* gradients, global float* errors, int idx, float by)
        {
            for (int x = 0; x < inputsSize; x++)
            {
                float$ value = inputs[x] * errors[idx];
                AddDiv2$(gradients, x, idx, inputsSize, value, by);
            }
        }

        inline void ComputeGradients_AddDivAddGradients$(global float$* inputs, int inputsSize, global float$* gradients, global float$* gradientSums, global float* errors, int idx, float by)
        {
            for (int x = 0; x < inputsSize; x++)
            {
                float$ value = inputs[x] * errors[idx];
                AddDivAdd2$(gradients, gradientSums, x, idx, inputsSize, value, by);
            }
        }

        );
    }
    else 
        program->Using(vault->GetCommonCode());

    //ComputeGradients_FF_Online_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(FF | Online)));

    //ComputeGradients_FF_Offline_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(FF | Offline)));

    //ComputeGradients_FF_OnlineOffline_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(FF | Online | Offline)));

    //ComputeGradients_BPTTPhase1_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(BPTTPhase1)));

    //ComputeGradients_BPTTPhase2_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(BPTTPhase2)));

    //ComputeGradients_BPTTPhase2_Offline_*
    program->AddCode(CreateKernelCode((GradientComputationFlags)(BPTTPhase2 | Offline)));
}

std::string OCLComputeGradientsKernel::CreateKernelCode(GradientComputationFlags flags)
{
    stringstream code;
    if (ctx->IsCPU()) code << CreateCPUKernelCode((GradientComputationFlags)(flags | CPU)); else code << CreateGPUKernelCode((GradientComputationFlags)(flags | GPU));
    return code.str();
}

void OCLComputeGradientsKernel::FillKernelPars(KernelPars& pars, GradientComputationFlags flags)
{
    pars.name = (flags & CPU) ?
        GetCPUNames().GetVersion(GradientComputationFlagsToVersion(flags)) :
        GetGPUNames().GetVersion(GradientComputationFlagsToVersion(flags));
    pars.calcGradients = (flags & Online | flags & BPTTPhase1 | flags & BPTTPhase2) != 0;
    pars.calcGradientSums = (flags & Offline) != 0;
    pars.ff = (flags & FF) != 0;
    pars.bpttp1 = (flags & BPTTPhase1) != 0;
    pars.bpttp2 = (flags & BPTTPhase2) != 0;
    pars.bptt = pars.bpttp1 || pars.bpttp2;

    assert(pars.calcGradients || pars.calcGradientSums);
}

std::string OCLComputeGradientsKernel::CreateKernelHeader(const KernelPars& pars)
{
    stringstream code;
    code << "kernel void " << pars.name->GetName() << "$(";
    code << "global float* errors,";
    if (pars.calcGradients) code << "global float* biasGradients,";
    if (pars.calcGradientSums) code << "global float* biasGradientSums,";
    code << "global float$* inputs,";
    code << "int inputsSize,";
    if (pars.calcGradients) code << "global float$* gradients";
    if (pars.calcGradientSums)
    {
        if (pars.calcGradients) code << ",";
        code << "global float$* gradientSums";
    }
    if (pars.bpttp2) code << ",float intItCount";
    code << ")";

    string hdr = code.str();

    return move(hdr);
}

std::string OCLComputeGradientsKernel::CreateCPUKernelCode(GradientComputationFlags flags)
{
    KernelPars pars;
    FillKernelPars(pars, flags);

    stringstream code;
    code << CreateKernelHeader(pars);

    code << "{";
    code << "int idx = get_global_id(0);";
    if (pars.ff)
    {
        if (pars.calcGradients) code << "biasGradients[idx] = errors[idx];";
        if (pars.calcGradientSums) code << "biasGradientSums[idx] += errors[idx];";
        if (pars.calcGradients && pars.calcGradientSums)
        {
            code << "ComputeGradients_SetAddGradients$(inputs, inputsSize, gradients, gradientSums, errors, idx);";
        }
        else if (pars.calcGradients)
        {
            code << "ComputeGradients_SetGradients$(inputs, inputsSize, gradients, errors, idx);";
        }
        else
        {
            assert(pars.calcGradientSums);
            code << "ComputeGradients_AddGradients$(inputs, inputsSize, gradientSums, errors, idx);";
        }
    }
    else if (pars.bpttp1)
    {
        assert(pars.calcGradients);
        if (pars.calcGradients) code << "biasGradients[idx] += errors[idx];";
        code << "ComputeGradients_AddGradients$(inputs, inputsSize, gradients, errors, idx);";
    }
    else if (pars.bpttp2)
    {
        assert(pars.calcGradients);
        if (pars.calcGradientSums)
        {
            code << "biasGradients[idx] += errors[idx];";
            code << "biasGradients[idx] /= intItCount;";
            code << "biasGradientSums[idx] += biasGradients[idx];";
            code << "ComputeGradients_AddDivAddGradients$(inputs, inputsSize, gradients, gradientSums, errors, idx, intItCount);";
        }
        else
        {
            code << "biasGradients[idx] += errors[idx];";
            code << "biasGradients[idx] /= intItCount;";
            code << "ComputeGradients_AddDivGradients$(inputs, inputsSize, gradients, errors, idx, intItCount);";
        }
    }
    code << "}";

    string codeStr = code.str();

    return move(codeStr);
}

std::string OCLComputeGradientsKernel::CreateGPUKernelCode(GradientComputationFlags flags)
{
    KernelPars pars;
    FillKernelPars(pars, flags);

    stringstream code;
    code << CreateKernelHeader(pars);

    code << "{";
    code << "int widx = get_global_id(0); int oidx = widx / inputsSize; int iidx = widx % inputsSize;";
    code << "if (iidx == 0)";
    code << "{";
    if (pars.ff)
    {
        if (pars.calcGradients) code << "biasGradients[oidx] = errors[oidx];";
        if (pars.calcGradientSums) code << "biasGradientSums[oidx] += errors[oidx];";
    }
    else if (pars.bpttp1)
    {
        code << "biasGradients[oidx] += errors[oidx];";
    }
    else if (pars.bpttp2)
    {
        assert(pars.calcGradients);
        if (pars.calcGradientSums)
        {
            code << "biasGradients[oidx] += errors[oidx];";
            code << "biasGradients[oidx] /= intItCount;";
            code << "biasGradientSums[oidx] += biasGradients[oidx];";
        }
        else
        {
            code << "biasGradients[oidx] += errors[oidx];";
            code << "biasGradients[oidx] /= intItCount;";
        }
    }
    code << "}";
    if (pars.ff)
    {
        code << "float$ v = " << "inputs[iidx] * errors[oidx];";
        if (pars.calcGradients) code << "gradients[widx] = v;";
        if (pars.calcGradientSums) code << "gradientSums[widx] += v;";
    }
    else if (pars.bpttp1)
    {
        code << "gradients[widx] += inputs[iidx] * errors[oidx];";
    }
    else if (pars.bpttp2)
    {
        assert(pars.calcGradients);
        if (pars.calcGradientSums)
        {
            code << "gradients[widx] += inputs[iidx] * errors[oidx];";
            code << "gradients[widx] /= intItCount;";
            code << "gradientSums[widx] += gradients[widx];";
        }
        else
        {
            code << "gradients[widx] += inputs[iidx] * errors[oidx];";
            code << "gradients[widx] /= intItCount;";
        }
    }
    code << "}";

    auto codeStr = code.str();

    return move(codeStr);
}

void OCLComputeGradientsKernel::ExecFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors)
{
    GradientComputationFlags flags = FF;
    if (gradients != null && biasGradients != null) flags = (GradientComputationFlags)(flags | Online);
    if (gradientSums != null && biasGradientSums != null) flags = (GradientComputationFlags)(flags | Offline);

    Exec(flags, state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors, 0);
}

void OCLComputeGradientsKernel::ExecBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors)
{
    GradientComputationFlags flags = BPTTPhase1;
    assert(gradients != null);
    assert(biasGradients != null);

    Exec(flags, state, inputs, gradients, biasGradients, null, null, errors, 0);
}

void OCLComputeGradientsKernel::ExecBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount)
{
    GradientComputationFlags flags = BPTTPhase2;
    assert(gradients != null);
    assert(biasGradients != null);
    if (gradientSums != null && biasGradientSums != null) flags = (GradientComputationFlags)(flags | Offline);

    Exec(flags, state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors, intItCount);
}

void OCLComputeGradientsKernel::Exec(GradientComputationFlags flags, NfObject* state, DeviceArrayFVecT* inputsV, DeviceArray2VecT* gradientsV, IDeviceArray* pBiasGradients, DeviceArray2VecT* gradientSumsV, IDeviceArray* pBiasGradientSums, IDeviceArray* pErrors, unsigned intItCount)
{
    flags = ctx->IsCPU() ? (GradientComputationFlags)(flags | CPU) : (GradientComputationFlags)(flags | GPU);
    KernelPars pars;
    FillKernelPars(pars, flags);

    unsigned size = inputsV->size();
    assert(!pars.calcGradients || gradientsV->size() == size);
    assert(!pars.calcGradientSums || gradientSumsV->size() == size);

    auto& exec = ((OCLComputationState*)state)->GetExec(0);
    auto& errors = ctx->ToBuffer1(pErrors);

    unsigned vectorSize = CalculateVectorSize(inputsV);

    for (unsigned i = 0; i < size; i++)
    {
        auto& inputs = ctx->ToBuffer1((*inputsV)[i]());
        float fiic = (float)intItCount;

        auto init = [=](Kernel& kernel)
        {
            int aidx = 0;
            kernel.setArg(aidx++, errors.GetCLBuffer());
            if (pars.calcGradients) kernel.setArg(aidx++, (ctx->ToBuffer1(pBiasGradients)).GetCLBuffer());
            if (pars.calcGradientSums) kernel.setArg(aidx++, (ctx->ToBuffer1(pBiasGradientSums)).GetCLBuffer());
            kernel.setArg(aidx++, inputs.GetCLBuffer());
            kernel.setArg(aidx++, inputs.GetSize() / vectorSize);
            if (pars.calcGradients) kernel.setArg(aidx++, (ctx->ToBuffer2((*gradientsV)[i])).GetCLBuffer());
            if (pars.calcGradientSums) kernel.setArg(aidx++, (ctx->ToBuffer2((*gradientSumsV)[i])).GetCLBuffer());
            if (pars.bpttp2) kernel.setArg(aidx++, fiic);
        };

        if (flags & CPU)
        {
            exec.Execute(
                program,
                (*pars.name)(vectorSize),
                vectorSize,
                init,
                errors.GetSize());
        }
        else
        {
            exec.Execute(
                program,
                (*pars.name)(vectorSize),
                vectorSize,
                init,
                (inputs.GetSize() / vectorSize) * errors.GetSize());
        }
    }
}