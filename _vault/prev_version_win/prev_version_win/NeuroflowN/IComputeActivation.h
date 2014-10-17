#pragma once

#include "Typedefs.h"
#include "NNMetadata.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class IComputeActivation : public NfObject
    {
    public:
        virtual NfObject* CreateComputationState() = 0;

        virtual void ComputeForward(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* biases, IDeviceArray* outputs, ActivationFunction function, float alpha) = 0;

        virtual void ComputeForwardRTLR(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* biases, IDeviceArray* outputs, IDeviceArray* netValueDerivates, ActivationFunction function, float alpha) = 0;

        virtual void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha) = 0;

        virtual void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, IDeviceArray* desiredOutputs, ActivationFunction function, float alpha) = 0;

        virtual void ComputeGradientsFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors) = 0;

        virtual void ComputeGradientsBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors) = 0;

        virtual void ComputeGradientsBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount) = 0;

        virtual void ComputeGradientsRTLR(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData2* data, IDeviceArray2* pValuesOfWeights, IDeviceArray* outputs, IDeviceArray* desiredOutputs, SequenceMarker seqMark) = 0;
    };
}