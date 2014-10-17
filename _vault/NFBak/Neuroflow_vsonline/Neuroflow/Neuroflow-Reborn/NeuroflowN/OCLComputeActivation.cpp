#include "stdafx.h"
#include "OCLComputeActivation.h"
#include "OCLProgramBuilder.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLKernelToExecute.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

void OCLComputeActivation::Build(OCLProgramBuilder& program, unsigned maxConnectionCount)
{
    // Common:
    DEFINE_OCL_PROGRAM(program,

    inline float$ Get2$(__global float$* values, int i1, int i2, int size1)
    {
        return values[GetIndex2(i1, i2, size1)];
    }

    inline void Set2$(__global float$* values, int i1, int i2, int size1, float$ value)
    {
        values[GetIndex2(i1, i2, size1)] = value;
    }

    inline void Add2$(__global float$* values, int i1, int i2, int size1, float$ value)
    {
        values[GetIndex2(i1, i2, size1)] += value;
    }

    inline void SetAdd2$(__global float$* values1, __global float$* values2, int i1, int i2, int size1, float$ value)
    {
        int index = GetIndex2(i1, i2, size1);
        values1[index] = value;
        values2[index] += value;
    }

    inline void AddDiv2$(__global float$* values, int i1, int i2, int size1, float$ value, float by)
    {
        int index = GetIndex2(i1, i2, size1);
        values[index] += value;
        values[index] /= by;
    }

    inline void AddDivAdd2$(__global float$* values1, __global float$* values2, int i1, int i2, int size1, float$ value, float by)
    {
        int index = GetIndex2(i1, i2, size1);
        values1[index] += value;
        values1[index] /= by;
        values2[index] += values1[index];
    }

    inline float$ Sigmoid$(float$ value, float alpha)
    {
        return (value * alpha) / (1.0f + fabs(value * alpha));
    }

    inline float$ SigmoidD$(float$ value, float alpha)
    {
        float$ a = fabs(value * alpha);
        return alpha * (1.0f / ((1.0f + a) * (1.0f + a)));
    }

    );

    // Compute forward:
    OCLComputeForwardKernel::Build(program, maxConnectionCount);
    OCLComputeInternalErrorsKernel::Build(program, maxConnectionCount);
    OCLComputeOutputErrorsKernel::Build(program);
    OCLComputeGradientsKernel::Build(program);
}

NfObject* OCLComputeActivation::CreateComputationState()
{
    return new OCLKernelToExecute();
}

void OCLComputeActivation::ComputeForward(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, ActivationFunction function, float alpha, bool isInputStable, bool isOutputStable)
{
    try
    {
        computeForwardKernel.Exec(state, inputs, weights, pBiases, pOutputs, function, alpha, isInputStable, isOutputStable);
    }
    catch (logic_error&)
    {
        throw;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeErrors(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha)
{
    try
    {
        computeOutputErrorsKernel.Exec(state, pOutputs, pErrors, pDesiredOutputs, function, alpha);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeErrors(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha)
{
    try
    {
        computeInternalErrorsKernel.Exec(state, pOutputs, pErrors, lowerWeights, lowerErrors, function, alpha);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable)
{
    try
    {
        computeGradientsKernel.ExecFF(state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors, isInputStable);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors, bool isInputStable)
{
    try
    {
        computeGradientsKernel.ExecBPTTPhase1(state, inputs, gradients, biasGradients, errors, isInputStable);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable, unsigned intItCount)
{
    try
    {
        computeGradientsKernel.ExecBPTTPhase2(state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors, isInputStable, intItCount);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}