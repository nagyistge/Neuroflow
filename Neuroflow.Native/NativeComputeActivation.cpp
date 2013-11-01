#include "stdafx.h"
#include "NativeComputeActivation.h"
#include "IComputeActivation.h"
#include "MUtil.h"
#include "NativeException.h"
#include "NativeObject.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;

System::IDisposable^ NativeComputeActivation::CreateComputationState()
{
    try
    {
        return gcnew NativeObject(computeActivation->CreateComputationState());
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeForward(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, ActivationFunction function, float alpha)
{
    try
    {
        computeActivation->ComputeForward(ToNative(state), ToNative(inputs), ToNative(weights), ToNative(biases), ToNative(outputs), ToNative(function), alpha);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeForwardRTLR(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, IDeviceArray^ netValueDerivates, ActivationFunction function, float alpha)
{
    try
    {
        computeActivation->ComputeForwardRTLR(ToNative(state), ToNative(inputs), ToNative(weights), ToNative(biases), ToNative(outputs), ToNative(netValueDerivates), ToNative(function), alpha);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, Marshaled<array<IDeviceArray2^>^>^ lowerWeights, Marshaled<array<IDeviceArray^>^>^ lowerErrors, ActivationFunction function, float alpha)
{
    try
    {
        auto lowerWeightsPtr = ToNative(lowerWeights);
        auto lowerErrorsPtr = ToNative(lowerErrors);

        computeActivation->ComputeErrors(ToNative(state), ToNative(outputs), ToNative(errors), lowerWeightsPtr, lowerErrorsPtr, ToNative(function), alpha);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}


void NativeComputeActivation::ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, IDeviceArray^ desiredOutputs, ActivationFunction function, float alpha)
{
    try
    {
        computeActivation->ComputeErrors(ToNative(state), ToNative(outputs), ToNative(errors), ToNative(desiredOutputs), ToNative(function), alpha);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeGradientsFF(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors)
{
    try
    {
        auto inputsPtr = ToNative(inputs);
        auto gradientsPtr = gradients != null ? ToNative(gradients) : null;
        auto gradientSumsPtr = gradientSums != null ? ToNative(gradientSums) : null;

        computeActivation->ComputeGradientsFF(
            ToNative(state), 
            inputsPtr, 
            gradientsPtr, 
            biasGradients != null ? ToNative(biasGradients) : null, 
            gradientSumsPtr,
            biasGradientSums != null ? ToNative(biasGradientSums) : null,
            ToNative(errors));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeGradientsBPTTPhase1(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, IDeviceArray^ errors)
{
    try
    {
        auto inputsPtr = ToNative(inputs);
        auto gradientsPtr = gradients != null ? ToNative(gradients) : null;

        computeActivation->ComputeGradientsBPTTPhase1(
            ToNative(state),
            inputsPtr,
            gradientsPtr,
            biasGradients != null ? ToNative(biasGradients) : null,
            ToNative(errors));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::ComputeGradientsBPTTPhase2(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors, int intItCount)
{
    try
    {
        auto inputsPtr = ToNative(inputs);
        auto gradientsPtr = gradients != null ? ToNative(gradients) : null;
        auto gradientSumsPtr = gradientSums != null ? ToNative(gradientSums) : null;

        computeActivation->ComputeGradientsBPTTPhase2(
            ToNative(state),
            inputsPtr,
            gradientsPtr,
            biasGradients != null ? ToNative(biasGradients) : null,
            gradientSumsPtr,
            biasGradientSums != null ? ToNative(biasGradientSums) : null,
            ToNative(errors),
            intItCount);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}


void NativeComputeActivation::ComputeGradientsRTLR(System::IDisposable^ state, Marshaled<array<array<RTLRLayerInfo^>^>^>^ inputLayerInfos, Marshaled<array<IDeviceArray^>^>^ netValueDerivates, Marshaled<RTLRComputationData^>^ data, Marshaled<array<IDeviceArray^>^>^ valueRelatedPBuffs, IDeviceArray^ outputs, IDeviceArray^ desiredOutputs)
{
    try
    {
        auto nOutputs = outputs != null ? ToNative(outputs) : null;
        auto nDOutputs = desiredOutputs != null ? ToNative(desiredOutputs) : null;

        assert(nOutputs == null && nDOutputs == null || nOutputs != null && nDOutputs != null);

        computeActivation->ComputeGradientsRTLR(
            ToNative(state),
            ToNative(inputLayerInfos),
            ToNative(netValueDerivates),
            ToNative(data),
            ToNative(valueRelatedPBuffs),
            nOutputs,
            nDOutputs);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeComputeActivation::CalculateGlobalError(System::IDisposable^ state, IDeviceArray^ desiredOutputs, IDeviceArray^ actualOutputs, IDeviceArray^ errorValue, IDeviceArray^ errorSumValue)
{
    throw gcnew System::NotImplementedException();
}