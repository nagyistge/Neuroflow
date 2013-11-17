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
        return gcnew NativeObject(Ptr->CreateComputationState());
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
        Ptr->ComputeForward(ToNative(state), ToNative(inputs), ToNative(weights), ToNative(biases), ToNative(outputs), ToNative(function), alpha);
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
        Ptr->ComputeForwardRTLR(ToNative(state), ToNative(inputs), ToNative(weights), ToNative(biases), ToNative(outputs), ToNative(netValueDerivates), ToNative(function), alpha);
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

        Ptr->ComputeErrors(ToNative(state), ToNative(outputs), ToNative(errors), lowerWeightsPtr, lowerErrorsPtr, ToNative(function), alpha);
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
        Ptr->ComputeErrors(ToNative(state), ToNative(outputs), ToNative(errors), ToNative(desiredOutputs), ToNative(function), alpha);
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

        Ptr->ComputeGradientsFF(
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

        Ptr->ComputeGradientsBPTTPhase1(
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

        Ptr->ComputeGradientsBPTTPhase2(
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


void NativeComputeActivation::ComputeGradientsRTLR(System::IDisposable^ state, Marshaled<array<array<RTLRLayerInfo^>^>^>^ inputLayerInfos, Marshaled<array<IDeviceArray^>^>^ netValueDerivates, Marshaled<RTLRComputationData^>^ data, Marshaled<array<IDeviceArray^>^>^ valueRelatedPBuffs, IDeviceArray^ outputs, IDeviceArray^ desiredOutputs, SequenceMarker seqMark)
{
    try
    {
        Ptr->ComputeGradientsRTLR(
            ToNative(state),
            ToNative(inputLayerInfos),
            ToNative(netValueDerivates),
            ToNative(data),
            ToNative(valueRelatedPBuffs),
            ToNative(outputs),
            ToNative(desiredOutputs),
            (NeuroflowN::SequenceMarker)((int)seqMark));
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