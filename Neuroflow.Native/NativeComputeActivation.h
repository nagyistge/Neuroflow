#pragma once

#include "Typedefs.h"
#include "NativePtr.h"
#include <assert.h>

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeComputeActivation : public NativePtr<NeuroflowN::IComputeActivation>, public IComputeActivation
        {
        public:
            NativeComputeActivation(NeuroflowN::IComputeActivation* computeActivation) :
                NativePtr(computeActivation, false)
            {
                assert(computeActivation != null);
            }

            virtual System::IDisposable^ CreateComputationState();

            virtual void ComputeForward(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, ActivationFunction function, float alpha);

            virtual void ComputeForwardRTLR(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, IDeviceArray^ netValueDerivates, ActivationFunction function, float alpha);

            virtual void ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, Marshaled<array<IDeviceArray2^>^>^ lowerWeights, Marshaled<array<IDeviceArray^>^>^ lowerErrors, ActivationFunction function, float alpha);

            virtual void ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, IDeviceArray^ desiredOutputs, ActivationFunction function, float alpha);

            virtual void ComputeGradientsFF(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors);

            virtual void ComputeGradientsBPTTPhase1(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, IDeviceArray^ errors);
            
            virtual void ComputeGradientsBPTTPhase2(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors, int intItCount);

            virtual void ComputeGradientsRTLR(System::IDisposable^ state, Marshaled<array<array<RTLRLayerInfo^>^>^>^ inputLayerInfos, Marshaled<array<IDeviceArray^>^>^ netValueDerivates, Marshaled<RTLRComputationData^>^ data, Marshaled<array<IDeviceArray^>^>^ valueRelatedPBuffs, IDeviceArray^ outputs, IDeviceArray^ desiredOutputs, SequenceMarker seqMark);

            virtual void CalculateGlobalError(System::IDisposable^ state, IDeviceArray^ desiredOutputs, IDeviceArray^ actualOutputs, IDeviceArray^ errorValue, IDeviceArray^ errorSumValue);
        };
    }
}
