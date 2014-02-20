#include "stdafx.h"
#include "OCLComputeForwardKernel.h"
#include "OCLProgramBuilder.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLKernelToExecute.h"
#include "OCL.h"

using namespace std;
using namespace NeuroflowN;
using namespace cl;

extern const char NeuroflowN::ComputeForwardTmpl [] = "ComputeForward_{0}_{1}_{2}";

void OCLComputeForwardKernel::Build(OCLProgramBuilder& program, unsigned max)
{
    DEFINE_OCL_PROGRAM(program,

    float ComputeForward_Sum$(__global float$* inputs, int inputsSize, __global float$* weights, int idx)
    {
        float$ sum = 0.0f;
        for (int x = 0; x < inputsSize; x++) sum += inputs[x] * Get2$(weights, x, idx, inputsSize);
        return SumComponents$(sum);
    }

    );

    for (unsigned size = 1; size <= max; size++)
    {
        auto cpuCode = CreateCPUKernelCode(size);
        auto gpuCode = CreateGPUKernelCode(size);

        program.Add(cpuCode);
        program.Add(gpuCode);
    }
}

std::string OCLComputeForwardKernel::CreateCPUKernelCode(unsigned size)
{
    auto names = CreateNames(ComputingUnit::CPU, size);

    auto factory = [size](const string& name, const char* calcCode)
    {
        stringstream code;
        code << "__kernel void " << name << "$(";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* inputs" << i << ",";
            code << "int inputsSize" << i << ",";
        }
        code << "__global float* biases,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* weights" << i << ",";
        }
        code << "__global float* outputs,";
        code << "float alpha)";
        code << "{";
        code << "int idx = get_global_id(0);";
        code << "float sum = biases[idx]";
        for (unsigned i = 0; i < size; i++)
        {
            code << " + ";
            code << "ComputeForward_Sum$(inputs" << i << ", inputsSize" << i << ", weights" << i << ", idx)";
        }
        code << ";";
        code << "outputs[idx] = " << calcCode << ";";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    code << factory(names.first, "Sigmoid(sum, alpha)");
    code << factory(names.second, "fmax(fmin(sum * alpha, alpha), -alpha)");

    return code.str();
}

std::string OCLComputeForwardKernel::CreateGPUKernelCode(unsigned size)
{
    auto names = CreateNames(ComputingUnit::GPU, size);

    auto factory = [size](const string& name, const char* calcCode)
    {
        stringstream code;
        code << "__kernel void " << name << "$(";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* inputs" << i << ",";
            code << "int inputsSize" << i << ",";
        }
        code << "__global float* biases,";
        for (unsigned i = 0; i < size; i++)
        {
            code << "__global float$* weights" << i << ",";
        }
        code << "__global float* outputs,";
        code << "float alpha)";
        code << "{";
        code << "__local int$ sum; int oidx = get_group_id(0); int iidx = get_local_id(0); int lsize = get_local_size(0); if (iidx == 0) sum = 0; barrier(CLK_LOCAL_MEM_FENCE);";
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
        code << "float sumf1 = SumComponents$(sumf);";
        code << "outputs[oidx] = " << calcCode << ";";
        code << "}";
        code << "}";
        return move(code.str());
    };

    stringstream code;
    code << factory(names.first, "Sigmoid(biases[oidx] + sumf1, alpha)");
    code << factory(names.second, "fmax(fmin((biases[oidx] + sumf1) * alpha, alpha), -alpha)");

    return code.str();
}

void OCLComputeForwardKernel::Exec(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, ActivationFunction function, float alpha, bool isInputStable, bool isOutputStable)
{
    unsigned size = (unsigned)inputs->size();
    assert(size == weights->size());

    auto exec = (OCLKernelToExecute*) state;
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
    };

    if (ctx->IsCPU())
    {
        if (function == ActivationFunction::Sigmoid)
        {
            exec->Execute(
                ctx,
                GetCPUNames(size).first(vectorSize),
                vectorSize,
                init,
                outputs.GetSize());
        }
        else
        {
            exec->Execute(
                ctx,
                GetCPUNames(size).second(vectorSize),
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
            exec->Execute(
                ctx,
                GetGPUNames(size).first(vectorSize),
                vectorSize,
                init,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
        else
        {
            exec->Execute(
                ctx,
                GetGPUNames(size).second(vectorSize),
                vectorSize,
                init,
                NDRange(sizes.first),
                NDRange(sizes.second));
        }
    }
}
