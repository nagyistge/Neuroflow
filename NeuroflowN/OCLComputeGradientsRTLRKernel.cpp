#include "stdafx.h"
#include "OCLComputeGradientsRTLRKernel.h"
#include "OCLIntCtx.h"
#include "OCLProgram.h"
#include "OCLVault.h"
#include "OCLComputationState.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLComputeGradientsRTLRKernel::OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
OCLVersionableKernelBase(ctx, "ComputeGradientsRTLR", { NoGradientCGRKV, OnlineCGRKV, OfflineCGRKV, OnlineOfflineCGRKV }, ctx->GetMaxConnectionCount(), true)
{
    Build(vault);
}

void OCLComputeGradientsRTLRKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsRTLRPrg");
    if (ctx->IsCPU()) program->Using(vault->GetNetCode()); else program->Using(vault->GetCommonCode());

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

std::string OCLComputeGradientsRTLRKernel::CreateCPUKernelCode(unsigned size)
{
    auto& names = GetCPUNames(size);

    auto createCode = [=](const char* name, ComputeGradientsRTLRKernelVersion version)
    {
        bool compGrads = version == OnlineCGRKV || version == OnlineOfflineCGRKV;
        bool compGradSums = version == OfflineCGRKV || version == OnlineOfflineCGRKV;

        stringstream code;
        code << "kernel void " << name << "$(";
        for (unsigned i = 0; i < size; i++)
        {
            code << "global float$* p_i_j_l_Values" << i;
            code << ",int p_i_j_l_ValuesSize" << i;
        }
        for (unsigned i = 0; i < size; i++)
        {
            code << ",global float$* weights" << i;
        }
        code << ", global float* p_i_j_k_Values";
        code << ", global float* layerNetValueDerivates";
        code << ", unsigned iValueIndex";
        code << ", float inputValue";
        if (version != NoGradientCGRKV)
        {
            code << ",float* desiredOutputs";
            code << ",float* outputs";
            if (compGrads) code << ",float* gradients";
            if (compGradSums) code << ",float* gradients";
        }
        code << ")";
        code << "{";

        code << "int kValueIndex = get_global_id(0);";
        if (version != NoGradientCGRKV)
        {
            code << "local int gradient;";
            code << "if (kValueIndex == 0) gradient = 0;";
            code << "barrier(CLK_LOCAL_MEM_FENCE);";
        }
        code << "float sum = (iValueIndex == kValueIndex ? inputValue : 0.0f)";
        for (unsigned i = 0; i < size; i++)
        {
            code << " + ";
            code << "ComputeForward_Sum$(p_i_j_l_Values" << i << ", p_i_j_l_ValuesSize" << i << ", weights" << i << ", kValueIndex)";
        }
        code << ";";
        code << "p_i_j_k_Values[kValueIndex] = layerNetValueDerivates[kValueIndex] * sum;";
        if (version != NoGradientCGRKV)
        {
            code << "int toAdd = ((desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p_i_j_k_Values[kValueIndex]) * D;";
            code << "AtomAdd(gradient, toAdd);";
            code << "barrier(CLK_LOCAL_MEM_FENCE);";
            code << "float fgradient = convert_float(gradient) / D;";
            if (compGrads) code << "gradients[gradientsIndex] = fgradient;";
            if (compGradSums)code << "gradientSums[gradientsIndex] += fgradient;";
        }
        code << "}";

        return code.str();
    };

    stringstream all;
    all << createCode(names.GetVersion(NoGradientCGRKV)->GetName().c_str(), NoGradientCGRKV);
    all << createCode(names.GetVersion(OnlineCGRKV)->GetName().c_str(), OnlineCGRKV);
    all << createCode(names.GetVersion(OfflineCGRKV)->GetName().c_str(), OfflineCGRKV);
    all << createCode(names.GetVersion(OnlineOfflineCGRKV)->GetName().c_str(), OnlineOfflineCGRKV);
    return all.str();
    /*

    kernel void K(
        global float$* p_i_j_l_Values0
        ,unsigned p_i_j_l_ValuesSize0
        ,global float$* p_i_j_l_Values1 
        ,unsigned p_i_j_l_ValuesSize1
        ,global float$* weights0
        ,global float$* weights1
        ,global float* p_i_j_k_Values
        ,global float* layerNetValueDerivates
        ,unsigned iValueIndex
        ,float inputValue
        @OPTIONAL, depend on: computeGradient
        ,float* desiredOutputs
        ,float* outputs
        ,float* gradients
        ,float* gradientSums
        ,unsigned gradientsIndex
        @OPTIONAL
        )
    {
        int kValueIndex = get_global_id(0);
        @OPTIONAL, depend on: computeGradient
        local int gradient;
        if (kValueIndex == 0) gradient = 0;
        barrier(CLK_LOCAL_MEM_FENCE);
        @OPTIONAL
        float sum = iValueIndex == kValueIndex ? inputValue : 0.0f;
        sum += ComputeForward_Sum$(p_i_j_l_Values0, p_i_j_l_Values0, weights0, kValueIndex);
        sum += ComputeForward_Sum$(p_i_j_l_Values1, p_i_j_l_Values1, weights1, kValueIndex);
        p_i_j_k_Values[kValueIndex] = layerNetValueDerivates[kValueIndex] * sum;
        @OPTIONAL, depend on: computeGradient
        int toAdd = ((desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p_i_j_k_Values[kValueIndex]) * D;
        AtomAdd(gradient, toAdd);
        barrier(CLK_LOCAL_MEM_FENCE);
        float fgradient = convert_float(gradient) / D;
        gradients[gradientsIndex] = fgradient;
        gradientSums[gradientsIndex] += fgradient;
        @OPTIONAL@
    }

    */
}

std::string OCLComputeGradientsRTLRKernel::CreateGPUKernelCode(unsigned size)
{
    return "kernel lofasz(global int* foo) { }";
}

void OCLComputeGradientsRTLRKernel::Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs)
{
    auto cState = (OCLComputationState*)state;

    unsigned kLayerSize = valueRelatedPBuffs->size();
    unsigned outputLayerIndex = kLayerSize - 1;
    for (int kLayerIndex = 0; kLayerIndex < kLayerSize; kLayerIndex++)
    {
        auto& layerNetValueDerivates = ctx->ToBuffer1((*netValueDerivates)[kLayerIndex]);
        auto& p_i_j_k_Values = ctx->ToBuffer1((*valueRelatedPBuffs)[kLayerIndex]);

        bool computeGradient = kLayerIndex == outputLayerIndex && outputs != null && desiredOutputs != null;
    }
}
