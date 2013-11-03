#include "stdafx.h"
#include "OCLComputeGradientsRTLRKernel.h"
#include "OCLIntCtx.h"
#include "OCLProgram.h"
#include "OCLVault.h"
#include "OCLComputationState.h"
#include "GetVectorSize.h"
#include "OCLDeviceArrayManagement.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLComputeGradientsRTLRKernel::OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const std::shared_ptr<OCLDeviceArrayManagement>& deviceArrayManagement) :
OCLVersionableKernelBase(ctx, "ComputeGradientsRTLR", { NoGradientCGRKV, CompGradientCGRKV, OnlineCGRKV, OfflineCGRKV, OnlineOfflineCGRKV }, ctx->GetMaxConnectionCount(), true),
deviceArrayManagement(deviceArrayManagement)
{
    Build(vault);

    tmpGradients = Buffer(
        ctx->GetContext(),
        CL_MEM_HOST_NO_ACCESS,
        sizeof(float) * 2048,
        nullptr);
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
        stringstream code;
        code << "kernel void " << name << "$(";
        if (version == NoGradientCGRKV || version == CompGradientCGRKV)
        {
            for (unsigned i = 0; i < size; i++)
            {
                if (i != 0) code << ",";
                code << "global float$* p_i_j_l_Values" << i;
                code << ",int p_i_j_l_ValuesSize" << i;
                code << ",global float$* weights" << i;
            }
            code << ", global float* p_i_j_k_Values";
            code << ", global float* layerNetValueDerivates";
            code << ", int iValueIndex";
            code << ", global float* inputs";
            code << ", int inputIndex";
            if (version == CompGradientCGRKV)
            {
                code << ",global float* desiredOutputs";
                code << ",global float* outputs";
                code << ",global float* tmpGradients";
            }
            code << ")";
            code << "{\n";

            code << "int kValueIndex = get_global_id(0);\n";
            /*if (version == CompGradientCGRKV)
            {
                code << "local int gradient;\n";
                code << "if (kValueIndex == 0) gradient = 0;\n";
                code << "barrier(CLK_LOCAL_MEM_FENCE);\n";
            }*/
            code << "float sum = ((iValueIndex == kValueIndex) ? (inputIndex != -1 ? inputs[inputIndex] : 1.0f) : 0.0f)\n";
            for (unsigned i = 0; i < size; i++)
            {
                code << " + ";
                code << "ComputeForward_Sum$(p_i_j_l_Values" << i << ", p_i_j_l_ValuesSize" << i << ", weights" << i << ", kValueIndex)";
            }
            code << ";\n";
            code << "p_i_j_k_Values[kValueIndex] = layerNetValueDerivates[kValueIndex] * sum;\n";
            if (version == CompGradientCGRKV)
            {
                /*code << "int toAdd = convert_int_rte(((desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p_i_j_k_Values[kValueIndex]) * D);\n";
                code << "AtomAdd(&gradient, toAdd);\n";
                code << "barrier(CLK_LOCAL_MEM_FENCE);\n";
                code << "float fgradient = convert_float(gradient) / D;\n";
                code << "tmpGradients[kValueIndex] = fgradient;\n";*/
                code << "tmpGradients[kValueIndex] = ((desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p_i_j_k_Values[kValueIndex]);\n";
            }
            code << "}\n\n";
        }
        else
        {
            bool compGrads = version == OnlineCGRKV || version == OnlineOfflineCGRKV;
            bool compGradSums = version == OfflineCGRKV || version == OnlineOfflineCGRKV;

            code << "global float* tmpGradients";
            code << ",int tmpGradientsSize";
            if (compGrads) code << ",global float* gradients";
            if (compGradSums) code << ",global float* gradientSums";
            code << ",int gradientIndex";
            code << ")";
            code << "{\n";
            code << "float fgradient = 0.0f;\n";
            code << "for (unsigned i = 0; i < tmpGradientsSize; i++) fgradient += tmpGradients[i];\n";
            if (compGrads) code << "gradients[gradientIndex] = fgradient;\n";
            if (compGradSums) code << "gradientSums[gradientIndex] += fgradient;\n";
            code << "}\n\n";
        }

        return code.str();
    };

    stringstream all;
    all << createCode(names.GetVersion(NoGradientCGRKV)->GetName().c_str(), NoGradientCGRKV);
    all << createCode(names.GetVersion(CompGradientCGRKV)->GetName().c_str(), CompGradientCGRKV);
    if (size == 1)
    {
        all << createCode(names.GetVersion(OnlineCGRKV)->GetName().c_str(), OnlineCGRKV);
        all << createCode(names.GetVersion(OfflineCGRKV)->GetName().c_str(), OfflineCGRKV);
        all << createCode(names.GetVersion(OnlineOfflineCGRKV)->GetName().c_str(), OnlineOfflineCGRKV);
    }
    return all.str();
    /*

    kernel void K1(
        global float$* p_i_j_l_Values0
        ,unsigned p_i_j_l_ValuesSize0
        ,global float$* weights0
        ,global float$* p_i_j_l_Values1 
        ,unsigned p_i_j_l_ValuesSize1
        ,global float$* weights1
        ,global float* p_i_j_k_Values
        ,global float* layerNetValueDerivates
        ,unsigned iValueIndex
        ,global float* inputs
        ,global unsigned inputIndex
        @OPTIONAL, depend on: computeGradient
        ,global float* outputs
        ,global float* desiredOutputs
        ,global float* tmpGradients
        @OPTIONAL
        )
    {
        int kValueIndex = get_global_id(0);
        @OPTIONAL, depend on: computeGradient
        local int gradient;
        if (kValueIndex == 0) gradient = 0;
        barrier(CLK_LOCAL_MEM_FENCE);
        @OPTIONAL
        float sum = iValueIndex == kValueIndex ? (inputs != NULL ? inputs[inputIndex] : 1.0f) : 0.0f;
        sum += ComputeForward_Sum$(p_i_j_l_Values0, p_i_j_l_Values0, weights0, kValueIndex);
        sum += ComputeForward_Sum$(p_i_j_l_Values1, p_i_j_l_Values1, weights1, kValueIndex);
        p_i_j_k_Values[kValueIndex] = layerNetValueDerivates[kValueIndex] * sum;
        @OPTIONAL, depend on: computeGradient
        int toAdd = convert_int_rte(((desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p_i_j_k_Values[kValueIndex]) * D);
        AtomAdd(gradient, toAdd);
        barrier(CLK_LOCAL_MEM_FENCE);
        float fgradient = convert_float(gradient) / D;
        tmpGradients[kValueIndex] = fgradient;
        @OPTIONAL@
    }

    kernel void K2B$(
        global float$* tmpGradients
        ,global float$* gradients
        ,global float$* gradientSums
        ,unsigned gradientIndex
    {
        int idx = get_global_id();
        local int$ gradient;
        if (idx == 0) gradient = 0;
        barrier(CLK_LOCAL_MEM_FENCE);
        AtomAdd(gradient, convert_int$_rte(tmpGradients[idx] * D));
        barrier(CLK_LOCAL_MEM_FENCE);
        float& fgradient = convert_float$(gradient) / D;
        gradients[gradientIndex] = fgradient;
        gradientSums[gradientIndex] += fgradient;
    }


    */
}

std::string OCLComputeGradientsRTLRKernel::CreateGPUKernelCode(unsigned size)
{
    /*code << "global float$* tmpGradients";
    if (compGrads) code << ",global float$* gradients";
    if (compGradSums) code << ",global float$* gradientSums";
    code << ",unsigned gradientIndex";
    code << ")";
    code << "{";
    code << "int idx = get_global_id();";
    code << "local int$ gradient;";
    code << "if (idx == 0) gradient = 0;";
    code << "barrier(CLK_LOCAL_MEM_FENCE);";
    code << "AtomAdd(&gradient, convert_int$_rte(tmpGradients[idx] * D));";
    code << "barrier(CLK_LOCAL_MEM_FENCE);";
    code << "float& fgradient = convert_float$(gradient) / D;";
    if (compGrads) code << "gradients[gradientIndex] = fgradient;";
    if (compGradSums) code << "gradientSums[gradientIndex] += fgradient;";
    code << "}";*/
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
        bool compGrads = computeGradient && (data->BiasGradients != null || data->Gradients != null);
        bool compGradSums = computeGradient && (data->BiasGradientSums != null || data->GradientSums != null);;
        auto& upperInfos_k = (*inputLayerInfos)[kLayerIndex];
        unsigned vectorSize, uCount;
        AnalyzeInfos(upperInfos_k, vectorSize, uCount);

        auto init1 = [=](Kernel& kernel)
        {
            int aidx = 0;
            for (auto& upperInfo_k : upperInfos_k)
            {
                if (upperInfo_k.IsElementOfU)
                {
                    int lLayerIndex = upperInfo_k.Index;
                    auto& p_i_j_l_Values = ctx->ToBuffer1((*valueRelatedPBuffs)[lLayerIndex]);
                    auto& weights = ctx->ToBuffer2(upperInfo_k.Weights);
                    kernel.setArg(aidx++, p_i_j_l_Values.GetCLBuffer());
                    kernel.setArg(aidx++, p_i_j_l_Values.GetSize() / vectorSize);
                    kernel.setArg(aidx++, weights.GetCLBuffer());
                }
            }
            kernel.setArg(aidx++, p_i_j_k_Values.GetCLBuffer());
            kernel.setArg(aidx++, layerNetValueDerivates.GetCLBuffer());
            if (data->ILayerIndex == kLayerIndex)
            {
                kernel.setArg(aidx++, data->IValueIndex);
                if (data->Inputs.is_initialized())
                {
                    kernel.setArg(aidx++, ctx->ToBuffer1(data->Inputs.get()()).GetCLBuffer());
                    kernel.setArg(aidx++, data->JValueIndex);
                }
                else
                {
                    kernel.setArg(aidx++, null);
                    kernel.setArg(aidx++, -1);
                }
            }
            else
            {
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, null);
                kernel.setArg(aidx++, -1);
            }
            if (computeGradient)
            {
                assert(outputs->GetSize() == p_i_j_k_Values.GetSize());
                assert(desiredOutputs->GetSize() == p_i_j_k_Values.GetSize());
                kernel.setArg(aidx++, ctx->ToBuffer1(desiredOutputs).GetCLBuffer());
                kernel.setArg(aidx++, ctx->ToBuffer1(outputs).GetCLBuffer());
                kernel.setArg(aidx++, tmpGradients);
            }
        };

        auto init2 = [=](Kernel& kernel)
        {
            assert(computeGradient);
            assert(compGrads || compGradSums);

            int aidx = 0;
            kernel.setArg(aidx++, tmpGradients);
            if (ctx->IsCPU()) kernel.setArg(aidx++, p_i_j_k_Values.GetSize());
            if (compGrads)
            {
                if (data->BiasGradients != null)
                {
                    kernel.setArg(aidx++, ctx->ToBuffer1(data->BiasGradients).GetCLBuffer());
                }
                else
                {
                    assert(data->Gradients != null);
                    kernel.setArg(aidx++, ctx->ToBuffer2(data->Gradients).GetCLBuffer());
                }
            }
            if (compGradSums)
            {
                if (data->BiasGradientSums != null)
                {
                    kernel.setArg(aidx++, ctx->ToBuffer1(data->BiasGradientSums).GetCLBuffer());
                }
                else
                {
                    assert(data->GradientSums != null);
                    kernel.setArg(aidx++, ctx->ToBuffer2(data->GradientSums).GetCLBuffer());
                }
            }
            kernel.setArg(aidx++, data->IJValueIndex);
        };

        ComputeGradientsRTLRKernelVersion kernel1Version, kernel2Version;
        kernel1Version = computeGradient ? CompGradientCGRKV : NoGradientCGRKV;
        kernel2Version = (compGrads && compGradSums) ? OnlineOfflineCGRKV : (compGrads ? OnlineCGRKV : OfflineCGRKV);

        if (ctx->IsCPU())
        {
            auto exec1 = cState->GetExec((kernel1Version + 1) * uCount);
            auto& name1 = (*GetCPUNames(uCount).GetVersion(kernel1Version))(vectorSize);
            auto& name2 = (*GetCPUNames().GetVersion(kernel2Version))(1);

            exec1->Execute(
                program,
                name1,
                vectorSize,
                init1,
                p_i_j_k_Values.GetSize());

            if (computeGradient)
            {
                auto exec2 = cState->GetExec((CompGradientCGRKV + 1) * uCount + kernel2Version);

                exec2->Execute(
                    program,
                    name2,
                    1,
                    init2,
                    1);
            }
        }
    }

    ctx->GetQueue().finish();

    //Event e;

    //ctx->GetQueue().enqueueMarkerWithWaitList(null, &e);

    //e.setCallback(
    //CL_COMPLETE,
    //[](cl_event event, cl_int status, void* userData)
    //{
    //    auto f = (function<void()>*)userData;
    //    try
    //    {
    //        if (status == CL_COMPLETE)
    //        {
    //            // Done
    //            (*f)();
    //        }
    //    }
    //    catch (...)
    //    {
    //    }
    //    //delete f;
    //},
    //new function<void()>(doIt));
}

void OCLComputeGradientsRTLRKernel::AnalyzeInfos(const RTLRLayerInfoVecT& infos, unsigned& vectorSize, unsigned& uCount) const
{
    vectorSize = 16;
    uCount = 0;
    for (auto& i : infos)
    {
        vectorSize = GetVectorSize(vectorSize, i.Size);
        if (i.IsElementOfU) uCount++;
    }
}
