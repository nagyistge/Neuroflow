#include "stdafx.h"
#include "OCLComputeGradientsRTLR2Kernel.h"
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

OCLComputeGradientsRTLR2Kernel::OCLComputeGradientsRTLR2Kernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
OCLKernelBase(ctx)
{
    Build(vault);
}

void OCLComputeGradientsRTLR2Kernel::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "ComputeGradientsRTLRPrg");
    program->Using(vault->GetNetCode());
    program->Using(vault->GetReduceCode());

    string code = CreateCode();
    program->AddCode(code);
}

std::string OCLComputeGradientsRTLR2Kernel::CreateCode()
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

    function<string(unsigned)> createPickMethodExpr = [&](unsigned parIndex)
    {
        string pidx = to_string(parIndex);
        if (parIndex == ctx->GetMaxLayerCount() - 1) return "v" + pidx;
        return "idx == " + pidx + " ? v" + pidx + " : (" + createPickMethodExpr(parIndex + 1) + ")";
    };

    auto declarePickMethod = [&](const string& type, const string& name)
    {
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
    };

    // --- BEGIN HELPERS ---

    code << declarePickMethod("global float$*", "PickFPValueByLayerIndex$");
    code << declarePickMethod("int", "PickIntValueByLayerIndex");

    code <<
        "inline global float* GetPValuesPtr(global float* pValuesOfWeights, int uLayersCount, int maxULayerSize, int kLayerIndex)\n"
        "{\n"
        "int ijValueIndex = get_group_id(0);\n"
        "return pValuesOfWeights + (ijValueIndex * uLayersCount * maxULayerSize) + (kLayerIndex * maxULayerSize);\n"
        "}\n"
        "void ComputeGradinetsRTLR_SetGradients(local float* tmpGradients, global float* gradients, global float* gradientSums)\n"
        "{\n"
        "Reduce_Sum(tmpGradients);\n"
        "if (get_local_id(0) == 0)\n"
        "{\n"
        "int gradientsIndex = get_group_id(0);\n"
        "if (gradients != null) gradients[gradientsIndex] = tmpGradients[0];\n"
        "if (gradientSums != null) gradientSums[gradientsIndex] += tmpGradients[0];\n"
        "}\n"
        "}\n";

    // --- END HELPERS

    // --- BEGIN HEADER ---

    code <<
        "kernel void ComputeGradientsRTLR$(\n"
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
        ", global float* inputs\n"
        ", int inputsSize\n"
        ", global float* outputs\n"
        ", global float* desiredOutputs\n"
        ", local float* tmpGradients\n"
        ", global float* gradients\n"
        ", global float* gradientSums)\n";

    // --- END HEADER ---

    // --- BEGIN BODY ---

    code <<
        "{\n"
        "int localId = get_local_id(0);\n"
        "int ijValueIndex = get_group_id(0);\n"
        "int iValueIndex = ijValueIndex / inputsSize;\n"
        "int jValueIndex = ijValueIndex % inputsSize;\n"
        "tmpGradients[localId] = 0.0f;\n"
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
        "float sum = (iLayerIndex == kLayerIndex && iValueIndex == kValueIndex) ? (inputs != null ? inputs[jValueIndex] : 1.0f) : 0.0f;\n";

    string incSumCode = "sum += ComputeForward_Sum$((float$*)GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, p_i_j_l_LayerIndex), p_i_j_l_LayerSize, weights, kValueIndex);\n";
    
    for (unsigned iidx = 0; iidx < ctx->GetMaxConnectionCount(); iidx++)
    {
        string iidxstr = to_string(iidx);
        if (iidx == 0)
        {
            code << "int p_i_j_l_LayerIndex = " << pickIntCall("p_i_j_l_LayerIndex_" + iidxstr) << ";\n";
            code << "int p_i_j_l_LayerSize = " << pickIntCall("p_i_j_l_LayerSize_" + iidxstr) << ";\n";
            code << "global float$* weights = " << pickFPCall("weights_" + iidxstr, true) << ";\n";
            code << incSumCode;
        }
        else
        {
            code << "p_i_j_l_LayerIndex = " << pickIntCall("p_i_j_l_LayerIndex_" + iidxstr) << ";\n";
            code <<
                "if (p_i_j_l_LayerIndex != -1)\n"
                "{\n";
            code << "p_i_j_l_LayerSize = " << pickIntCall("p_i_j_l_LayerSize_" + iidxstr) << ";\n";
            code << "weights = " << pickFPCall("weights_" + iidxstr, true) << ";\n";
            code << incSumCode;
            code << "}\n";
        }
    }

    code <<
        "global float* netDerivValues = " << pickFPCall("netDerivValues", false) << ";\n"
        "float p = netDerivValues[kValueIndex] * sum;\n"
        "GetPValuesPtr(pValuesOfWeights, uLayersCount, maxULayerSize, kLayerIndex)[kValueIndex] = p;\n"
        "if (computeGradients) tmpGradients[localId] += (desiredOutputs[kValueIndex] - outputs[kValueIndex]) * p;\n"
        "}\n"
        "kLayerAndValueIndex++;\n"
        "}\n"
        "barrier(CLK_LOCAL_MEM_FENCE);\n"
        "ComputeGradinetsRTLR_SetGradients(tmpGradients, gradients, gradientSums);\n";
    code << "}";

    return code.str();
}