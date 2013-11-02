#include "stdafx.h"
#include "OCLComputeActivation.h"
#include "OCLIntCtx.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLComputationState.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

NfObject* OCLComputeActivation::CreateComputationState()
{
    return new OCLComputationState();
}

void OCLComputeActivation::ComputeForward(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, ActivationFunction function, float alpha)
{
    try
    {
        computeForwardKernel.Exec(state, inputs, weights, pBiases, pOutputs, null, function, alpha);
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

void OCLComputeActivation::ComputeForwardRTLR(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, IDeviceArray* pNetValueDerivates, ActivationFunction function, float alpha)
{
    try
    {
        computeForwardKernel.Exec(state, inputs, weights, pBiases, pOutputs, pNetValueDerivates, function, alpha);
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

void OCLComputeActivation::ComputeGradientsFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors)
{
    try
    {
        computeGradientsKernel.ExecFF(state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors)
{
    try
    {
        computeGradientsKernel.ExecBPTTPhase1(state, inputs, gradients, biasGradients, errors);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount)
{
    try
    {
        computeGradientsKernel.ExecBPTTPhase2(state, inputs, gradients, biasGradients, gradientSums, biasGradientSums, errors, intItCount);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLComputeActivation::ComputeGradientsRTLR(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs)
{
    try
    {
        computeGradientsRTLRKernel.Exec(state, inputLayerInfos, netValueDerivates, data, valueRelatedPBuffs, outputs, desiredOutputs);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}