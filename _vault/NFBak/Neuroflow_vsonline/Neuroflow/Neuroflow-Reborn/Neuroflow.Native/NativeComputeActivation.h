#pragma once

#include "Typedefs.h"

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeComputeActivation : public IComputeActivation
        {
            NeuroflowN::IComputeActivation* computeActivation;

        public:
            NativeComputeActivation(NeuroflowN::IComputeActivation* computeActivation) :
                computeActivation(computeActivation)
            {
            }

            virtual System::IDisposable^ CreateComputationState();

            virtual void ComputeForward(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, ActivationFunction function, float alpha, bool isInputStable, bool isOutputStable);

            virtual void ComputeForwardRTLR(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ weights, IDeviceArray^ biases, IDeviceArray^ outputs, IDeviceArray^ netValueDerivates, ActivationFunction function, float alpha, bool isInputStable, bool isOutputStable);

            virtual void ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, Marshaled<array<IDeviceArray2^>^>^ lowerWeights, Marshaled<array<IDeviceArray^>^>^ lowerErrors, ActivationFunction function, float alpha);

            virtual void ComputeErrors(System::IDisposable^ state, IDeviceArray^ outputs, IDeviceArray^ errors, IDeviceArray^ desiredOutputs, ActivationFunction function, float alpha);

            virtual void ComputeGradientsFF(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors, bool isInputStable);

            virtual void ComputeGradientsBPTTPhase1(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, IDeviceArray^ errors, bool isInputStable);
            
            virtual void ComputeGradientsBPTTPhase2(System::IDisposable^ state, Marshaled<array<DeviceArrayFactory^>^>^ inputs, Marshaled<array<IDeviceArray2^>^>^ gradients, IDeviceArray^ biasGradients, Marshaled<array<IDeviceArray2^>^>^ gradientSums, IDeviceArray^ biasGradientSums, IDeviceArray^ errors, bool isInputStable, int intItCount);

            virtual void ComputeGradientsRTLR(Marshaled<RTLRComputationData^>^ data, Marshaled<array<IDeviceArray^>^>^ valueRelatedPBuffs, IDeviceArray^ outputs, IDeviceArray^ desiredOutputs);

            virtual void CalculateGlobalError(System::IDisposable^ state, IDeviceArray^ desiredOutputs, IDeviceArray^ actualOutputs, IDeviceArray^ errorValue, IDeviceArray^ errorSumValue);
        };
    }
}
