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
OCLVersionableKernelBase(ctx, "ComputeGradientsRTLR"),
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
        "global* float$ p_i_j_l_Values_%_~"
        ",int p_i_j_l_ValuesSize_%_~"
        ",global* float$ weights_%_~";

    string layerParsTmpl2 =
        ",global* float p_i_j_k_Values_~"
        ",int p_i_j_k_ValuesSize_~"
        ",global* float netDerivValues_~";

    stringstream hdr;
    hdr << "kernel void " << name << "$(";

    for (unsigned layerIndex = 0; layerIndex < ctx->GetMaxLayerCount(); layerIndex++)
    {
        for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
        {
            if (layerIndex != 0 && inputIndex != 0) hdr << ',';
            hdr << boost::replace_all_copy(boost::replace_all_copy(layerParsTmpl1, "%", to_string(inputIndex)), "~", to_string(layerIndex));
        }

        hdr << boost::replace_all_copy(layerParsTmpl2, "~", to_string(layerIndex));
    }

    hdr << ",int iLayerIndex";
    hdr << ",int iValueIndex";
    hdr << ",global float* inputs";
    hdr << ",global unsigned inputIndex";
    hdr << ",global float* outputs";
    hdr << ",global float* desiredOutputs";
    hdr << ",local float* tmpGradients";
    hdr << ",global float* gradients";
    hdr << ",global float* gradientSums";
    hdr << ",int gradientIndex)";

    string hdrStr = hdr.str();
    return hdrStr;
}

std::string OCLComputeGradientsRTLRKernel::CreateCallCode_ComputeGradinetsRTLR_Layer_CPU(unsigned layerIndex)
{
    string tmpl = 
        "p_i_j_l_Values_%_~"
        ",p_i_j_l_ValuesSize_%_~"
        ",weights_%_~";

    stringstream code;
    code << "ComputeGradinetsRTLR_Layer_CPU(";
    for (unsigned inputIndex = 0; inputIndex < ctx->GetMaxConnectionCount(); inputIndex++)
    {
        if (inputIndex != 0) code << ',';
        code << boost::replace_all_copy(boost::replace_all_copy(tmpl, "%", to_string(inputIndex)), "~", to_string(layerIndex));
    }
}

std::string OCLComputeGradientsRTLRKernel::CreateCPUKernelCode()
{
    auto& names = GetCPUNames();

    auto createCode = [=](const char* name)
    {
        stringstream code;
        code << GetKernelHeader(name);
        code << "tmpGradients[get_global_id(0)] = 0.0f;";
        code << "int kLayerIndex;";
        code << "bool isLastLayer;";

        for (int layerIndex = 0; layerIndex < ctx->GetMaxLayerCount(); layerIndex++)
        {
            
        }

        code << "ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums, gradientIndex);";
    };
}

std::string OCLComputeGradientsRTLRKernel::CreateGPUKernelCode()
{
    return "kernel lofasz(global int* foo) { }";
}

void OCLComputeGradientsRTLRKernel::Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs)
{
    auto cState = (OCLComputationState*)state;

    unsigned kLayerSize = valueRelatedPBuffs->size();
    unsigned outputLayerIndex = kLayerSize - 1;

    ctx->GetQueue().finish();
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
