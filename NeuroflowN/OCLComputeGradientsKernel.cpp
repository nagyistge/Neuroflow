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

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_Online_CPU = OCLVectorKernelName("ComputeGradients_FF_Online_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_Online_GPU = OCLVectorKernelName("ComputeGradients_FF_Online_GPU");

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_Offline_CPU = OCLVectorKernelName("ComputeGradients_FF_Offline_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_Offline_GPU = OCLVectorKernelName("ComputeGradients_FF_Offline_GPU");

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_OnlineOffline_CPU = OCLVectorKernelName("ComputeGradients_FF_OnlineOffline_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_FF_OnlineOffline_GPU = OCLVectorKernelName("ComputeGradients_FF_OnlineOffline_GPU");

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase1_CPU = OCLVectorKernelName("ComputeGradients_BPTTPhase1_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase1_GPU = OCLVectorKernelName("ComputeGradients_BPTTPhase1_GPU");

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase2_CPU = OCLVectorKernelName("ComputeGradients_BPTTPhase2_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase2_GPU = OCLVectorKernelName("ComputeGradients_BPTTPhase2_GPU");

OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase2_Offline_CPU = OCLVectorKernelName("ComputeGradients_BPTTPhase2_Offline_CPU");
OCLVectorKernelName OCLComputeGradientsKernel::ComputeGradients_BPTTPhase2_Offline_GPU = OCLVectorKernelName("ComputeGradients_BPTTPhase2_Offline_GPU");

void OCLComputeGradientsKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsPrg");
    if (ctx->IsCPU())
    {
        program->Using(vault->GetNetCode());
        ADD_OCL_CODE(program,

        inline void ComputeGradients_SetGradients$(__global float$* inputs, int inputsSize, __global float$* gradients, __global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++) Set2$(gradients, x, idx, inputsSize, inputs[x] * errors[idx]);
        }

        inline void ComputeGradients_AddGradients$(__global float$* inputs, int inputsSize, __global float$* gradientSums, __global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++) Add2$(gradientSums, x, idx, inputsSize, inputs[x] * errors[idx]);
        }

        inline void ComputeGradients_SetAddGradients$(__global float$* inputs, int inputsSize, __global float$* gradients, __global float$* gradientSums, __global float* errors, int idx)
        {
            for (int x = 0; x < inputsSize; x++)
            {
                float$ value = inputs[x] * errors[idx];
                SetAdd2$(gradients, gradientSums, x, idx, inputsSize, value);
            }
        }

        inline void ComputeGradients_AddDivGradients$(__global float$* inputs, int inputsSize, __global float$* gradients, __global float* errors, int idx, float by)
        {
            for (int x = 0; x < inputsSize; x++)
            {
                float$ value = inputs[x] * errors[idx];
                AddDiv2$(gradients, x, idx, inputsSize, value, by);
            }
        }

        inline void ComputeGradients_AddDivAddGradients$(__global float$* inputs, int inputsSize, __global float$* gradients, __global float$* gradientSums, __global float* errors, int idx, float by)
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

const OCLVectorKernelName& OCLComputeGradientsKernel::GetKernelName(GradientComputationFlags flags)
{
    if (flags & FF)
    {
        if (flags & Online)
        {
            if (flags & Offline)
            {
                if (flags & CPU)
                {
                    return ComputeGradients_FF_OnlineOffline_CPU;
                }
                else if (flags & GPU)
                {
                    return ComputeGradients_FF_OnlineOffline_GPU;
                }
            }
            else
            {
                // Online && !Offline
                if (flags & CPU)
                {
                    return ComputeGradients_FF_Online_CPU;
                }
                else if (flags & GPU)
                {
                    return ComputeGradients_FF_Online_GPU;
                }
            }
        }
        else
        {
            // !Online
            if (flags & Offline)
            {
                if (flags & CPU)
                {
                    return ComputeGradients_FF_Offline_CPU;
                }
                else if (flags & GPU)
                {
                    return ComputeGradients_FF_Offline_GPU;
                }
            }
        }
    }
    else if (flags & BPTTPhase1)
    {
        if (flags & CPU)
        {
            return ComputeGradients_BPTTPhase1_CPU;
        }
        else if (flags & GPU)
        {
            return ComputeGradients_BPTTPhase1_GPU;
        }
    }
    else if (flags & BPTTPhase2)
    {
        if (flags & Offline)
        {
            if (flags & CPU)
            {
                return ComputeGradients_BPTTPhase2_Offline_CPU;
            }
            else if (flags & GPU)
            {
                return ComputeGradients_BPTTPhase2_Offline_GPU;
            }
        }
        else
        {
            if (flags & CPU)
            {
                return ComputeGradients_BPTTPhase2_CPU;
            }
            else if (flags & GPU)
            {
                return ComputeGradients_BPTTPhase2_GPU;
            }
        }
    }

    ThrowUnknownFlagsEx(flags);

    throw_logic_error("This ain't gonna happen.");
}

std::string OCLComputeGradientsKernel::CreateKernelCode(GradientComputationFlags flags)
{
    stringstream code;
    if (ctx->IsCPU()) code << CreateCPUKernelCode((GradientComputationFlags)(flags | CPU)); else code << CreateGPUKernelCode((GradientComputationFlags)(flags | GPU));
    return code.str();
}

void OCLComputeGradientsKernel::FillKernelPars(KernelPars& pars, GradientComputationFlags flags, bool fillName)
{
    if (fillName) pars.name = make_shared<string>(GetKernelName(flags)(1));
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
    code << "__kernel void " << *pars.name << "$(";
    code << "__global float* errors,";
    if (pars.calcGradients) code << "__global float* biasGradients,";
    if (pars.calcGradientSums) code << "__global float* biasGradientSums,";
    code << "__global float$* inputs,";
    code << "int inputsSize,";
    if (pars.calcGradients) code << "__global float$* gradients";
    if (pars.calcGradientSums)
    {
        if (pars.calcGradients) code << ",";
        code << "__global float$* gradientSums";
    }
    if (pars.bpttp2) code << ",float intItCount";
    code << ",unsigned limit";
    code << ")";

    string hdr = code.str();

    return move(hdr);
}

std::string OCLComputeGradientsKernel::CreateCPUKernelCode(GradientComputationFlags flags)
{
    KernelPars pars;
    FillKernelPars(pars, flags, true);

    stringstream code;
    code << CreateKernelHeader(pars);

    code << "{";
    code << "int idx = get_global_id(0);";
    code << "int gs = get_global_size(0);";
    code << "while (idx < limit)";
    code << "{";
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
    code << "idx += gs;";
    code << "}";
    code << "}";

    string codeStr = code.str();

    return move(codeStr);
}

std::string OCLComputeGradientsKernel::CreateGPUKernelCode(GradientComputationFlags flags)
{
    KernelPars pars;
    FillKernelPars(pars, flags, true);

    stringstream code;
    code << CreateKernelHeader(pars);

    code << "{";
    code << "int idx = get_global_id(0);";
    code << "int gs = get_global_size(0);";
    code << "while (idx < limit)";
    code << "{";
    code << "int widx = idx; int oidx = widx / inputsSize; int iidx = widx % inputsSize;";
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
    code << "idx += gs;";
    code << "}";
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
    KernelPars pars;
    FillKernelPars(pars, flags, false);

    unsigned size = inputsV->size();
    assert(!pars.calcGradients || gradientsV->size() == size);
    assert(!pars.calcGradientSums || gradientSumsV->size() == size);

    auto& exec = ((OCLComputationState*)state)->GetExec(0);
    auto& errors = ctx->ToBuffer1(pErrors);

    unsigned vectorSize = CalculateVectorSize(inputsV);

    for (unsigned i = 0; i < size; i++)
    {
        auto& inputs = ctx->ToBuffer1((*inputsV)[i]());
        unsigned runSize = ctx->IsCPU() ? errors.GetSize() : (inputs.GetSize() / vectorSize) * errors.GetSize();
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
            kernel.setArg(aidx++, runSize);
        };

        if (ctx->IsCPU())
        {
            exec.Execute(
                program,
                GetKernelName((GradientComputationFlags)(flags | CPU))(vectorSize),
                vectorSize,
                init,
                runSize);
        }
        else
        {
            exec.Execute(
                program,
                GetKernelName((GradientComputationFlags)(flags | GPU))(vectorSize),
                vectorSize,
                init,
                runSize);
        }
    }
}