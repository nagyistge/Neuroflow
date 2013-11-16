#include "stdafx.h"
#include "OCLComputeGradientsRTLRKernel.h"
#include "OCLIntCtx.h"
#include "OCLProgram.h"
#include "OCLVault.h"
#include "OCLComputationState.h"
#include "GetVectorSize.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLOutOfOrderQueue.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLComputeGradientsRTLRKernel::OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
OCLVersionableKernelBase(ctx, "ComputeGradientsRTLR")
{
    Build(vault);
}

void OCLComputeGradientsRTLRKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsRTLRPrg");
    if (ctx->IsCPU()) program->Using(vault->GetNetCode()); else program->Using(vault->GetCommonCode());
    program->Using(vault->GetReduceCode());

    ADD_OCL_CODE(program,
        void ComputeGradinetsRTLR_SetGradients(local float* tmpGradients, global float* gradients, global float* gradientSums, int gradientsIndex)
        {
            Reduce_Sum(tmpGradients);

            if (get_local_id(0) == 0)
            {
                if (gradients != null) gradients[gradientsIndex] = tmpGradients[0];
                if (gradientSums != null) gradientSums[gradientsIndex] += tmpGradients[0];
            }
        }
    );

    if (ctx->IsCPU())
    {
        auto cpuCode = CreateCPUKernelCode();
        program->AddCode(cpuCode);
    }
    else
    {
        auto gpuCode = CreateGPUKernelCode();
        program->AddCode(gpuCode);
    }
}

std::string OCLComputeGradientsRTLRKernel::GetKernelHeader(const char* name)
{
    string layerParsTmpl1 =
        "global float$* p_i_j_l_Values_{i}_{l}\n"
        ",int p_i_j_l_ValuesSize_{i}_{l}\n"
        ",global float$* weights_{i}_{l}\n";

    string layerParsTmpl2 =
        ",global float* p_i_j_k_Values_{l}\n"
        ",int p_i_j_k_ValuesSize_{l}\n"
        ",global float* netDerivValues_{l}\n";

    stringstream hdr;
    hdr << "kernel void " << name << "$(\n";

    for (unsigned layerIndex = 0; layerIndex < ctx->GetMaxLayerCount(); layerIndex++)
    {
        for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
        {
            if (!(inputIndex == 0 && layerIndex == 0)) hdr << ',';
            hdr << ReplaceIndexesInTemplate(layerParsTmpl1, inputIndex, layerIndex);
        }

        hdr << ReplaceIndexesInTemplate(layerParsTmpl2, layerIndex);
    }

    hdr << ",int iLayerIndex\n";
    hdr << ",int iValueIndex\n";
    hdr << ",global float* inputs\n";
    hdr << ",int inputIndex\n";
    hdr << ",global float* outputs\n";
    hdr << ",global float* desiredOutputs\n";
    hdr << ",local float* tmpGradients\n";
    hdr << ",global float* gradients\n";
    hdr << ",global float* gradientSums\n";
    hdr << ",int gradientIndex)\n";

    string hdrStr = hdr.str();
    return hdrStr;
}

std::string OCLComputeGradientsRTLRKernel::CreateCode_ComputeGradinetsRTLR_Layer_CPU()
{
    string tmpl = 
        "global float$* p_i_j_l_Values_{i}\n"
        ",int p_i_j_l_ValuesSize_{i}\n"
        ",global float$* weights_{i}\n";

    stringstream code;
    code << "void ComputeGradinetsRTLR_Layer_CPU$(\n";
    for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
    {
        if (inputIndex != 0) code << ",";
        code << ReplaceIndexesInTemplate(tmpl, inputIndex, null);
    }
    code << 
        ",global float* p_i_j_k_Values\n"
        ",int p_i_j_k_ValuesSize\n"
        ",global float* netDerivValues\n"
        ",int iValueIndex\n"
        ",global float* inputs\n"
        ",int inputIndex\n"
        ",local float* tmpGradients\n"
        ",global float* outputs\n"
        ",global float* desiredOutputs)\n";
    code <<
        "{\n"
        "int localSize = get_local_size(0);\n"
        "int localId = get_local_id(0);\n"
        "int block = p_i_j_k_ValuesSize / localSize + (p_i_j_k_ValuesSize % localSize != 0 ? 1 : 0);\n"
        "int kValueIndex = localId * block;\n"
        "int max = kValueIndex + block;\n"
        "if (max > p_i_j_k_ValuesSize) max = p_i_j_k_ValuesSize;\n"
        "while (kValueIndex < max)\n"
        "{\n"
        "float sum = iValueIndex == kValueIndex ? (inputs != null ? inputs[inputIndex] : 1.0f) : 0.0f; \n";
    
    for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
    {
        if (inputIndex != 0) code << "if (p_i_j_l_Values_" << inputIndex << " != null) ";
        code << "sum += ComputeForward_Sum$(p_i_j_l_Values_" << inputIndex << ", p_i_j_l_ValuesSize_" << inputIndex << ", weights_" << inputIndex << ", kValueIndex);\n";
    };
    
    code <<
        "float p = netDerivValues[kValueIndex] * sum;\n"
        "p_i_j_k_Values[kValueIndex] = p;\n"
        "if (tmpGradients != null) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;\n"
        "kValueIndex++;\n"
        "}\n"
        "}\n";

    return code.str();
}

std::string OCLComputeGradientsRTLRKernel::CreateCallCode_ComputeGradinetsRTLR_Layer_CPU(unsigned layerIndex)
{
    string tmpl = 
        "p_i_j_l_Values_{i}_{l}\n"
        ",p_i_j_l_ValuesSize_{i}_{l}\n"
        ",weights_{i}_{l}\n";

    stringstream code;
    code << "ComputeGradinetsRTLR_Layer_CPU$(\n";
    for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
    {
        if (inputIndex != 0) code << ',';
        code << ReplaceIndexesInTemplate(tmpl, inputIndex, layerIndex);
    }

    code << ReplaceIndexesInTemplate(",p_i_j_k_Values_{l}\n,p_i_j_k_ValuesSize_{l}\n,netDerivValues_{l}\n", layerIndex);

    code <<
        ",iLayerIndex == kLayerIndex ? iValueIndex : -1\n"
        ",inputs\n"
        ",inputIndex\n"
        ",(isLastLayer && outputs != null) ? tmpGradients : null\n" // Because there is no way to null tmpGradients by clSetKerenlArgs
        ",isLastLayer ? outputs : null\n"
        ",isLastLayer ? desiredOutputs : null);\n";

    return code.str();
}

std::string OCLComputeGradientsRTLRKernel::CreateCPUKernelCode()
{
    auto& names = GetCPUNames();
    stringstream code;
    code << CreateCode_ComputeGradinetsRTLR_Layer_CPU();

    auto createCode = [=](const char* name)
    {
        stringstream code;
        code << GetKernelHeader(name);
        code << "{\n";
        code << "tmpGradients[get_local_id(0)] = 0.0f;\n";
        code << "barrier(CLK_LOCAL_MEM_FENCE);\n";
        code << "int kLayerIndex;\n";
        code << "bool isLastLayer;\n";

        for (int layerIndex = 0; layerIndex < ctx->GetMaxLayerCount(); layerIndex++)
        {
            if (layerIndex != 0)
            {
                code << "if (!isLastLayer)\n"
                    "{\n";
            }

            code << "kLayerIndex = " << layerIndex << ";\n";
            if (layerIndex == ctx->GetMaxLayerCount() - 1)
            {
                code << "isLastLayer =  true;\n";
            }
            else
            {
                code << "isLastLayer =  p_i_j_k_Values_" << layerIndex + 1 << " == null;\n";
            }
            code << CreateCallCode_ComputeGradinetsRTLR_Layer_CPU(layerIndex);
            code << "barrier(CLK_LOCAL_MEM_FENCE);\n";
            if (layerIndex != 0) code << "}\n";
        }

        code << "ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums, gradientIndex);\n";
        code << "}\n";

        return code.str();
    };

    code << createCode(names.GetVersion()->GetName().c_str());
    string codeStr = code.str();
    
    return codeStr;
}

std::string OCLComputeGradientsRTLRKernel::CreateGPUKernelCode()
{
    return "kernel lofasz(global int* foo) { }";
}

void OCLComputeGradientsRTLRKernel::Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs, SequenceMarker seqMark)
{
    auto cState = (OCLComputationState*)state;
    auto exec = cState->GetExec(0, true);
    unsigned kLayerSize = valueRelatedPBuffs->size();
    unsigned vectorSize = CalculateVectorSize(*inputLayerInfos);
    unsigned workSize = CalculateWorkSize(*valueRelatedPBuffs);
    bool hasError = outputs != null && desiredOutputs != null;
    bool compGrads = hasError && (data->BiasGradients != null || data->Gradients != null);
    bool compGradSums = hasError && (data->BiasGradientSums != null || data->GradientSums != null);

    auto init = [=](Kernel& kernel)
    {
        int aidx = 0;

        int lSet = 0;
        for (int kLayerIndex = 0; kLayerIndex < kLayerSize; kLayerIndex++)
        {
            auto layerNetValueDerivates = ctx->ToBuffer1((*netValueDerivates)[kLayerIndex]);
            auto p_i_j_k_Values = ctx->ToBuffer1((*valueRelatedPBuffs)[kLayerIndex]);
            auto& upperInfos_k = (*inputLayerInfos)[kLayerIndex];
           
            unsigned iSet = 0;
            for (auto& upperInfo_k : upperInfos_k)
            {
                if (upperInfo_k.IsElementOfU)
                {
                    int lLayerIndex = upperInfo_k.Index;
                    auto p_i_j_l_Values = ctx->ToBuffer1((*valueRelatedPBuffs)[lLayerIndex]);
                    auto weights = ctx->ToBuffer2(upperInfo_k.Weights);
                    kernel.setArg(aidx++, p_i_j_l_Values->GetCLBuffer());
                    kernel.setArg(aidx++, p_i_j_l_Values->GetSize() / vectorSize);
                    kernel.setArg(aidx++, weights->GetCLBuffer());
                    iSet++;
                }
            }
            while (iSet < ctx->GetMaxConnectionCount())
            {
                kernel.setArg(aidx++, null);
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, null);
                iSet++;
            }
            kernel.setArg(aidx++, p_i_j_k_Values->GetCLBuffer());
            kernel.setArg(aidx++, p_i_j_k_Values->GetSize());
            kernel.setArg(aidx++, layerNetValueDerivates->GetCLBuffer());
            lSet++;
        }
        while (lSet < ctx->GetMaxLayerCount())
        {
            for (unsigned i = 0; i < ctx->GetMaxConnectionCount(); i++)
            {
                kernel.setArg(aidx++, null);
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, null);
            }
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, -1);
            kernel.setArg(aidx++, null);
            lSet++;
        }

        kernel.setArg(aidx++, data->ILayerIndex);
        kernel.setArg(aidx++, data->IValueIndex);
        if (data->Inputs.is_initialized())
        {
            kernel.setArg(aidx++, ctx->ToBuffer1(data->Inputs.get()())->GetCLBuffer());
            kernel.setArg(aidx++, data->JValueIndex);
        }
        else
        {
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, -1);
        }
        if (hasError)
        {
            kernel.setArg(aidx++, ctx->ToBuffer1(outputs)->GetCLBuffer());
            kernel.setArg(aidx++, ctx->ToBuffer1(desiredOutputs)->GetCLBuffer());
            kernel.setArg(aidx++, workSize, null);
        }
        else
        {
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, sizeof(float), null);
        }
        if (compGrads)
        {
            if (data->BiasGradients != null)
            {
                kernel.setArg(aidx++, ctx->ToBuffer1(data->BiasGradients)->GetCLBuffer());
            }
            else
            {
                assert(data->Gradients != null);
                kernel.setArg(aidx++, ctx->ToBuffer2(data->Gradients)->GetCLBuffer());
            }
        }
        else
        {
            kernel.setArg(aidx++, null);
        }
        if (compGradSums)
        {
            if (data->BiasGradientSums != null)
            {
                kernel.setArg(aidx++, ctx->ToBuffer1(data->BiasGradientSums)->GetCLBuffer());
            }
            else
            {
                assert(data->GradientSums != null);
                kernel.setArg(aidx++, ctx->ToBuffer2(data->GradientSums)->GetCLBuffer());
            }
        }
        else
        {
            kernel.setArg(aidx++, null);
        }
        kernel.setArg(aidx++, data->IJValueIndex);
    };
    
    if (seqMark == SequenceMarker::Begin) ctx->GetOutOfOrderQueue()->End();

    if (ctx->IsCPU())
    {
        exec->Execute(program, (*GetCPUNames().GetVersion())(vectorSize), vectorSize, init, workSize, workSize);
    }

    if (seqMark == SequenceMarker::End) ctx->GetOutOfOrderQueue()->End();
}

unsigned OCLComputeGradientsRTLRKernel::CalculateVectorSize(const RTLRLayerInfoVecVecT& infos) const
{
    unsigned vectorSize = 16;
    for (auto& li : infos)
    {
        for (auto& i : li)
        {
            vectorSize = GetVectorSize(vectorSize, i.Size);
        }
    }
    return vectorSize;
}

unsigned OCLComputeGradientsRTLRKernel::CalculateWorkSize(const DeviceArrayVecT& valueRelatedPBuffs) const
{
    double sum = 0.0;
    double count = 0.0;
    for (auto& b : valueRelatedPBuffs)
    {
        sum += b->GetSize();
        count++;
    }
    return ctx->GetOptimalLocalSizeForOneWorkgroup(sum / count, 1);
}
