using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public interface IComputeActivation : IComputationStateFactory
    {
        void ComputeForward(IDisposable state, Marshaled<DeviceArrayFactory[]> inputs, Marshaled<IDeviceArray2[]> weights, IDeviceArray biases, IDeviceArray outputs, ActivationFunction function, float alpha);

        void ComputeForwardRTLR(IDisposable state, Marshaled<DeviceArrayFactory[]> inputs, Marshaled<IDeviceArray2[]> weights, IDeviceArray biases, IDeviceArray outputs, IDeviceArray netValueDerivates, ActivationFunction function, float alpha);

        void ComputeErrors(IDisposable state, IDeviceArray outputs, IDeviceArray errors, Marshaled<IDeviceArray2[]> lowerWeights, Marshaled<IDeviceArray[]> lowerErrors, ActivationFunction function, float alpha);

        void ComputeErrors(IDisposable state, IDeviceArray outputs, IDeviceArray errors, IDeviceArray desiredOutputs, ActivationFunction function, float alpha);

        void ComputeGradientsFF(IDisposable state, Marshaled<DeviceArrayFactory[]> inputs, Marshaled<IDeviceArray2[]> gradients, IDeviceArray biasGradients, Marshaled<IDeviceArray2[]> gradientSums, IDeviceArray biasGradientSums, IDeviceArray errors);

        void ComputeGradientsBPTTPhase1(IDisposable state, Marshaled<DeviceArrayFactory[]> inputs, Marshaled<IDeviceArray2[]> gradients, IDeviceArray biasGradients, IDeviceArray errors);

        void ComputeGradientsBPTTPhase2(IDisposable state, Marshaled<DeviceArrayFactory[]> inputs, Marshaled<IDeviceArray2[]> gradients, IDeviceArray biasGradients, Marshaled<IDeviceArray2[]> gradientSums, IDeviceArray biasGradientSums, IDeviceArray errors, int intItCount);

        void ComputeGradientsRTLR(IDisposable state, Marshaled<RTLRLayerInfo[][]> inputLayerInfos, Marshaled<IDeviceArray[]> netValueDerivates, Marshaled<RTLRComputationData> data, Marshaled<IDeviceArray[]> valueRelatedPBuffs, IDeviceArray outputs, IDeviceArray desiredOutputs, SequenceMarker seqMark);

        void ComputeGradientsRTLR2(IDisposable state, Marshaled<RTLRLayerInfo[][]> inputLayerInfos, Marshaled<IDeviceArray[]> netValueDerivates, Marshaled<RTLRComputationData2> data, Marshaled<IDeviceArray2> pValuesOfWeights, IDeviceArray outputs, IDeviceArray desiredOutputs, SequenceMarker seqMark);

        void CalculateGlobalError(IDisposable state, IDeviceArray desiredOutputs, IDeviceArray actualOutputs, IDeviceArray errorValue, IDeviceArray errorSumValue);
    }
}
