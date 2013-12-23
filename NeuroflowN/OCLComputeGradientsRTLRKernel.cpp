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

OCLVectorKernelName OCLComputeGradientsRTLRKernel::name = OCLVectorKernelName("ComputeGradientsRTLR");

OCLComputeGradientsRTLRKernel::OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
OCLKernelBase(ctx)
{
    Build(vault);
}

void OCLComputeGradientsRTLRKernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsRTLRPrg");
    program->Using(vault->GetNetCode());
    program->Using(vault->GetReduceCode());

    program->AddCode(DeclarePickMethod("int", "PickIntValueByLayerIndex"));

    ADD_OCL_CODE(program,
    inline global float* GetPValuesPtr(global float* pValuesOfWeights, int uLayersCount, int maxULayerSize, int kLayerIndex)
    {
        int ijValueIndex = get_group_id(0);
        return pValuesOfWeights + (ijValueIndex * uLayersCount * maxULayerSize) + (kLayerIndex * maxULayerSize);
    }

    void ComputeGradinetsRTLR_SetGradients(local float* tmpGradients, global float* gradients, global float* gradientSums)
    {
        Reduce2D_Sum(tmpGradients);

        if (get_local_id(0) == 0 && get_local_id(1) == 0)
        {
            int ijValueIndex = get_group_id(0);
            if (gradients != null) gradients[ijValueIndex] = tmpGradients[0];
            if (gradientSums != null) gradientSums[ijValueIndex] += tmpGradients[0];
        }
    });

    ADD_OCL_CODE(program,
    void ComputeForwardRTLR_Sum$(global float$* inputs, int inputsSize, global float$* weights, int idx, local float* tmpSum, bool start)
    {
        int localId1 = get_local_id(1);
        int localSize1 = get_local_size(1);

        if (start) tmpSum[localId1] = 0.0f;
        barrier(CLK_LOCAL_MEM_FENCE);

        int block = inputsSize / localSize1 + (inputsSize % localSize1 != 0 ? 1 : 0);
        int idx = localId1 * block;
        int max = idx + block;
        if (max > size) max = size;
        while (idx <  max)
        {
            tmpSum[localId1] += SumComponents$(inputs[x] * Get2$(weights, x, idx, inputsSize));

            idx++;
        }

        barrier(CLK_LOCAL_MEM_FENCE);
    }

    void ReduceRTLR_Sum$(local float* tmpSum)
    {
        int localId1 = get_local_id(1);
        int localSize1 = get_local_size(1);

        for (int offset = localSize / 2; offset > 0; offset = offset / 2)
        {
            if (localId1 < offset)
            {
                tmpSum[localId1] += tmpSum[localId1 + offset];
            }

            barrier(CLK_LOCAL_MEM_FENCE);
        }
    });

    string code = CreateCode();
    program->AddCode(code);
}

std::string OCLComputeGradientsRTLRKernel::DeclarePickMethod(const string& type, const string& name) const
{
    function<string(unsigned)> createPickMethodExpr = [&](unsigned parIndex)
    {
        string pidx = to_string(parIndex);
        if (parIndex == ctx->GetMaxLayerCount() - 1) return "v" + pidx;
        return "idx == " + pidx + " ? v" + pidx + " : (" + createPickMethodExpr(parIndex + 1) + ")";
    };

    stringstream r;
    r << "inline ";
    r << type << " ";
    r << name << "(";
    for (unsigned lidx = 0; lidx < ctx->GetMaxLayerCount(); lidx++)
    {
        r << type << " v" << lidx << ", ";
    }
    r << "int idx)\n";
    r << "{\n";
    r << "return " << createPickMethodExpr(0) << ";\n";
    r << "}\n";
    return r.str();
}

std::string OCLComputeGradientsRTLRKernel::CreateCode()
{
    stringstream code;

    auto pickIntCall = [&](const string& parName)
    {
        stringstream r;
        r << "PickIntValueByLayerIndex(";
        for (unsigned lidx = 0; lidx < ctx->GetMaxLayerCount(); lidx++)
        {
            r << parName << "_" << lidx << ", ";
        }
        r << "kLayerIndex);";
        return r.str();
    };

    auto pickFPCall = [&](const string& parName, bool vector)
    {
        stringstream r;
        r << "PickFPValueByLayerIndex";
        if (vector) r << "$";
        r << "(";
        for (unsigned lidx = 0; lidx < ctx->GetMaxLayerCount(); lidx++)
        {
            r << parName << "_" << lidx << ", ";
        }
        r << "kLayerIndex);";
        return r.str();
    };

    // --- BEGIN HELPERS ---

    code << DeclarePickMethod("global float$*", "PickFPValueByLayerIndex$");

    // --- END HELPERS ---

    // --- BEGIN HEADER ---

    code <<
        "kernel void " << name.GetName() << "$(\n"
        "global float* pValuesOfWeights\n"
        ", int uLayersCount\n"
        ", int maxULayerSize\n";

    for (unsigned lidx = 0; lidx < ctx->GetMaxLayerCount(); lidx++)
    {
        for (unsigned iidx = 0; iidx < ctx->GetMaxConnectionCount(); iidx++)
        {
            string tmpl =
                ", int p_i_j_l_LayerIndex_{i}_{l}\n"
                ", int p_i_j_l_LayerSize_{i}_{l}\n"
                ", global float$* weights_{i}_{l}\n";

            code << ReplaceIndexesInTemplate(tmpl, iidx, lidx);
        }

        string tmpl =
            ", int p_i_j_k_LayerSize_{l}\n"
            ", global float* netDerivValues_{l}\n";

        code << ReplaceIndexesInTemplate(tmpl, lidx);
    }

    code <<
        ", int iLayerIndex\n"
        ", global float* inputs\n"
        ", int inputsSize\n"
        ", global float* outputs\n"
        ", global float* desiredOutputs\n"
        ", local float* tmpGradients\n"
        ", local float* tmpSums\n"
        ", global float* gradients\n"
        ", global float* gradientSums)\n";

    // --- END HEADER ---

    // --- BEGIN BODY ---

    code <<
        "{\n"
        "int localId = get_local_id(0);\n"
        "int localSize = get_local_size(0);\n"
        "int localId1 = get_local_id(1);\n"
        "int localSize1 = get_local_size(1);\n"
        "int ijValueIndex = get_group_id(0);\n"
        "int iValueIndex = ijValueIndex / inputsSize;\n"
        "int jValueIndex = ijValueIndex % inputsSize;\n"
        "if (localId1 == 0) tmpGradients[localId] = 0.0f;\n"
        "barrier(CLK_LOCAL_MEM_FENCE);\n"
        "int pValuesOfWeightsSize2 = uLayersCount * maxULayerSize;\n"
        "int block = pValuesOfWeightsSize2 / localSize + (pValuesOfWeightsSize2 % localSize != 0 ? 1 : 0);\n"
        "int kLayerAndValueIndex = localId * block;\n"
        "int max = kLayerAndValueIndex + block;\n"
        "if (max > pValuesOfWeightsSize2) max = pValuesOfWeightsSize2;\n"
        "while (kLayerAndValueIndex < max)\n"
        "{\n"
        "int kLayerIndex = kLayerAndValueIndex / maxULayerSize;\n"
        "int kValueIndex = kLayerAndValueIndex % maxULayerSize;\n";

    code << "int kLayerSize = " << pickIntCall("p_i_j_k_LayerSize") << ";\n";

    code <<
        "if (kValueIndex < kLayerSize)\n"
        "{\n"
        "bool computeGradient = (kLayerIndex == uLayersCount - 1) && outputs != null && desiredOutputs != null;\n"
        "float sum = 0.0f;\n"
        "local float* tmpSum = tmpSums + kLayerIndex * localSize1;\n";

    string incSumCode0 = "ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, true);\n";
    string incSumCode1 = "ComputeForwardRTLR_Sum$((global float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex, tmpSum, false);\n";

    for (unsigned iidx = 0; iidx < ctx->GetMaxConnectionCount(); iidx++)
    {
        string iidxstr = to_string(iidx);
        if (iidx == 0)
        {
            code << "int p_i_j_l_LayerIndex = " << pickIntCall("p_i_j_l_LayerIndex_" + iidxstr) << ";\n";
            code << "int p_i_j_l_LayerSize = " << pickIntCall("p_i_j_l_LayerSize_" + iidxstr) << ";\n";
            code << "global float$* weights = " << pickFPCall("weights_" + iidxstr, true) << ";\n";
            code << incSumCode0;
        }
        else
        {
            code << "p_i_j_l_LayerIndex = " << pickIntCall("p_i_j_l_LayerIndex_" + iidxstr) << ";\n";
            code <<
                "if (p_i_j_l_LayerIndex != -1)\n"
                "{\n";
            code << "p_i_j_l_LayerSize = " << pickIntCall("p_i_j_l_LayerSize_" + iidxstr) << ";\n";
            code << "weights = " << pickFPCall("weights_" + iidxstr, true) << ";\n";
            code << incSumCode1;
            code << "}\n";
        }
    }

    code <<
        "ReduceRTLR_Sum$(tmpSum);\n"
        "if (localId1 == 0)\n"
        "{\n"
        "float sum = tmpSum[0];\n"
        "sum += (iLayerIndex == kLayerIndex && iValueIndex == kValueIndex) ? (inputs != null ? inputs[jValueIndex] : 1.0f) : 0.0f;\n"
        "global float* netDerivValues = " << pickFPCall("netDerivValues", false) << ";\n"
        "float p = netDerivValues[kValueIndex] * sum;\n"
        "GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, kLayerIndex)[kValueIndex] = p;\n"
        "if (computeGradient) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;\n"
        "}\n"
        "}\n"
        "kLayerAndValueIndex++;\n"
        "barrier(CLK_LOCAL_MEM_FENCE);\n"
        "}\n"
        "if (gradients != null || gradientSums != null)\n"
        "{\n"
        "ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums);\n"
        "}\n";

    code << "}";

    return code.str();
}

void OCLComputeGradientsRTLRKernel::Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData2* data, IDeviceArray2* pValuesOfWeights, IDeviceArray* outputs, IDeviceArray* desiredOutputs, SequenceMarker seqMark)
{
    bool ooo = ctx->IsCPU();
    auto cState = (OCLComputationState*)state;
    auto pValuesOfWeightsBuff = ctx->ToBuffer2(pValuesOfWeights);
    auto exec = cState->GetExec(0, ooo);
    unsigned vectorSize = CalculateVectorSize(inputLayerInfos);
    unsigned localSize = CalculateLocalSize(netValueDerivates);
    unsigned localSize2 = 16;
    unsigned globalSize = localSize * pValuesOfWeightsBuff->GetSize1();
    bool hasError = outputs != null && desiredOutputs != null;
    bool compGrads = hasError && (data->BiasGradients != null || data->Gradients != null);
    bool compGradSums = hasError && (data->BiasGradientSums != null || data->GradientSums != null);

    auto init = [=](Kernel& kernel)
    {
        int aidx = 0;
        kernel.setArg(aidx++, pValuesOfWeightsBuff->GetCLBuffer());
        kernel.setArg(aidx++, data->ULayersCount);
        kernel.setArg(aidx++, data->MaxULayerSize);

        int lSet = 0;
        for (int kLayerIndex = 0; kLayerIndex < data->ULayersCount; kLayerIndex++)
        {
            auto& upperInfos_k = (*inputLayerInfos)[kLayerIndex];

            unsigned iSet = 0;
            for (auto& upperInfo_k : upperInfos_k)
            {
                if (upperInfo_k.IsElementOfU)
                {
                    int lLayerIndex = upperInfo_k.Index;
                    auto weights = ctx->ToBuffer2(upperInfo_k.Weights);
                    kernel.setArg(aidx++, lLayerIndex);
                    kernel.setArg(aidx++, upperInfo_k.Size / vectorSize);
                    kernel.setArg(aidx++, weights->GetCLBuffer());
                    iSet++;
                }
            }
            while (iSet < ctx->GetMaxConnectionCount())
            {
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, null);
                iSet++;
            }
            auto layerNetValueDerivates = ctx->ToBuffer1((*netValueDerivates)[kLayerIndex]);
            kernel.setArg(aidx++, layerNetValueDerivates->GetSize());
            kernel.setArg(aidx++, layerNetValueDerivates->GetCLBuffer());
            lSet++;
        }
        while (lSet < ctx->GetMaxLayerCount())
        {
            for (unsigned i = 0; i < ctx->GetMaxConnectionCount(); i++)
            {
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, -1);
                kernel.setArg(aidx++, null);
            }
            kernel.setArg(aidx++, -1);
            kernel.setArg(aidx++, null);
            lSet++;
        }

        kernel.setArg(aidx++, data->ILayerIndex);

        if (data->Inputs.is_initialized())
        {
            auto inputs = ctx->ToBuffer1(data->Inputs.get()());
            kernel.setArg(aidx++, inputs->GetCLBuffer());
            kernel.setArg(aidx++, inputs->GetSize());
        }
        else
        {
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, 1);
        }

        if (hasError)
        {
            kernel.setArg(aidx++, ctx->ToBuffer1(outputs)->GetCLBuffer());
            kernel.setArg(aidx++, ctx->ToBuffer1(desiredOutputs)->GetCLBuffer());
        }
        else
        {
            kernel.setArg(aidx++, null);
            kernel.setArg(aidx++, null);
        }

        if (compGrads || compGradSums)
        {
            kernel.setArg(aidx++, localSize, null);
        }
        else
        {
            kernel.setArg(aidx++, 1, null);
        }

        kernel.setArg(aidx++, localSize2 * data->MaxULayerSize, null);

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
    };

    if (ooo && seqMark == SequenceMarker::Begin) ctx->GetOutOfOrderQueue()->Begin();
    exec->Execute(program, name(vectorSize), vectorSize, init, NDRange(globalSize, localSize2), NDRange(localSize, localSize2));
    if (ooo && seqMark == SequenceMarker::End) ctx->GetOutOfOrderQueue()->End();
}

unsigned OCLComputeGradientsRTLRKernel::CalculateVectorSize(const RTLRLayerInfoVecVecT* infos) const
{
    unsigned vectorSize = 16;
    for (auto& li : *infos)
    {
        for (auto& i : li)
        {
            if (i.IsElementOfU) vectorSize = GetVectorSize(vectorSize, i.Size);
        }
    }
    return vectorSize;
}

unsigned OCLComputeGradientsRTLRKernel::CalculateLocalSize(const DeviceArrayVecT* netValueDerivates) const
{
    double sum = 0.0;
    double count = 0.0;
    for (auto& b : *netValueDerivates)
    {
        sum += b->GetSize();
        count++;
    }
    return ctx->GetOptimalLocalSizeForOneWorkgroup(sum / count, 1);
}