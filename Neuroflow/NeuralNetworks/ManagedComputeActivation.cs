using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    unsafe internal class ManagedComputeActivation : IComputeActivation
    {
        public IDisposable CreateComputationState()
        {
            return null;
        }

        public void ComputeForward(IDisposable state, Marshaled<DeviceArrayFactory[]> inputsM, Marshaled<IDeviceArray2[]> weightsM, IDeviceArray biases, IDeviceArray outputs, ActivationFunction function, float alpha)
        {
            var inputs = inputsM.Instance();
            var weights = weightsM.Instance();

            Debug.Assert(inputs.Length != 0 && inputs.Length == weights.Length);

            var mOutputs = outputs.ToManaged();
            var mBiases = (ManagedArray)biases;

            fixed (float* pOutputs = mOutputs.InternalArray, pBiases = mBiases.InternalArray)
            {
                var outputsPtr = mOutputs.ToPtr(pOutputs);
                var biasesPtr = mBiases.ToPtr(pBiases);

                if (function == ActivationFunction.Sigmoid)
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = biasesPtr[oIdx];
                        for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                        {
                            var inputsMA = (inputs[lIdx]()).ToManaged();
                            var weightsMA = (ManagedArray2)weights[lIdx];

                            Debug.Assert(inputsMA.Size != 0 && inputsMA.Size == weightsMA.Size1);
                            Debug.Assert(outputs.Size == weightsMA.Size2);

                            fixed (float* pInputs = inputsMA.InternalArray, pWeights = weightsMA.InternalArray)
                            {
                                sum += ComputeForward_Sum(inputsMA.ToPtr(pInputs), weightsMA.ToPtr(pWeights), oIdx);
                            }
                        }

                        outputsPtr[oIdx] = Sigmoid(sum, alpha);
                    }
                }
                else
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = biasesPtr[oIdx];
                        for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                        {
                            var inputsMA = (inputs[lIdx]()).ToManaged();
                            var weightsMA = (ManagedArray2)weights[lIdx];

                            Debug.Assert(inputsMA.Size != 0 && inputsMA.Size == weightsMA.Size1);
                            Debug.Assert(outputs.Size == weightsMA.Size2);

                            fixed (float* pInputs = inputsMA.InternalArray, pWeights = weightsMA.InternalArray)
                            {
                                sum += ComputeForward_Sum(inputsMA.ToPtr(pInputs), weightsMA.ToPtr(pWeights), oIdx);
                            }
                        }

                        outputsPtr[oIdx] = Math.Min(Math.Max(sum * alpha, -alpha), alpha);
                    }
                }
            }
        }

        public void ComputeForwardRTLR(IDisposable state, Marshaled<DeviceArrayFactory[]> inputsM, Marshaled<IDeviceArray2[]> weightsM, IDeviceArray biases, IDeviceArray outputs, IDeviceArray netValueDerivates, ActivationFunction function, float alpha)
        {
            var inputs = inputsM.Instance();
            var weights = weightsM.Instance();

            Debug.Assert(inputs.Length != 0 && inputs.Length == weights.Length);

            var mOutputs = outputs.ToManaged();
            var mNVDerivs = netValueDerivates.ToManaged();
            var mBiases = (ManagedArray)biases;

            fixed (float* pOutputs = mOutputs.InternalArray, pBiases = mBiases.InternalArray, pNVDerivs = mNVDerivs.InternalArray)
            {
                var outputsPtr = mOutputs.ToPtr(pOutputs);
                var biasesPtr = mBiases.ToPtr(pBiases);
                var nvDerivsPtr = mNVDerivs.ToPtr(pNVDerivs);

                if (function == ActivationFunction.Sigmoid)
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = biasesPtr[oIdx];
                        for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                        {
                            var inputsMA = (inputs[lIdx]()).ToManaged();
                            var weightsMA = (ManagedArray2)weights[lIdx];

                            Debug.Assert(inputsMA.Size != 0 && inputsMA.Size == weightsMA.Size1);
                            Debug.Assert(outputs.Size == weightsMA.Size2);

                            fixed (float* pInputs = inputsMA.InternalArray, pWeights = weightsMA.InternalArray)
                            {
                                sum += ComputeForward_Sum(inputsMA.ToPtr(pInputs), weightsMA.ToPtr(pWeights), oIdx);
                            }
                        }

                        outputsPtr[oIdx] = Sigmoid(sum, alpha);
                        nvDerivsPtr[oIdx] = SigmoidD(sum, alpha);
                    }
                }
                else
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = biasesPtr[oIdx];
                        for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                        {
                            var inputsMA = (inputs[lIdx]()).ToManaged();
                            var weightsMA = (ManagedArray2)weights[lIdx];

                            Debug.Assert(inputsMA.Size != 0 && inputsMA.Size == weightsMA.Size1);
                            Debug.Assert(outputs.Size == weightsMA.Size2);

                            fixed (float* pInputs = inputsMA.InternalArray, pWeights = weightsMA.InternalArray)
                            {
                                sum += ComputeForward_Sum(inputsMA.ToPtr(pInputs), weightsMA.ToPtr(pWeights), oIdx);
                            }
                        }

                        outputsPtr[oIdx] = Math.Min(Math.Max(sum * alpha, -alpha), alpha);
                        nvDerivsPtr[oIdx] = alpha;
                    }
                }
            }
        }

        public unsafe void ComputeErrors(IDisposable state, IDeviceArray outputs, IDeviceArray errors, IDeviceArray desiredOutputs, ActivationFunction function, float alpha)
        {
            var mOutputs = outputs.ToManaged();
            var mErrors = (ManagedArray)errors;
            var mDesiredOutputs = desiredOutputs.ToManaged();

            fixed (float* pOutputs = mOutputs.InternalArray, pErrors = mErrors.InternalArray, pDesiredOutputs = mDesiredOutputs.InternalArray)
            {
                var outputsPtr = mOutputs.ToPtr(pOutputs);
                var errorsPtr = mErrors.ToPtr(pErrors);
                var desiredOutputsPtr = mDesiredOutputs.ToPtr(pDesiredOutputs);

                if (function == ActivationFunction.Sigmoid)
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        errorsPtr[oIdx] = (desiredOutputsPtr[oIdx] - outputsPtr[oIdx]) * SigmoidD(outputsPtr[oIdx], alpha);
                    }
                }
                else
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        errorsPtr[oIdx] = (desiredOutputsPtr[oIdx] - outputsPtr[oIdx]) * alpha;
                    }
                }
            }
        }

        public void ComputeErrors(IDisposable state,  IDeviceArray outputs, IDeviceArray errors, Marshaled<IDeviceArray2[]> lowerWeightsM, Marshaled<IDeviceArray[]> lowerErrorsM, ActivationFunction function, float alpha)
        {
            var lowerWeights = lowerWeightsM.Instance();
            var lowerErrors = lowerErrorsM.Instance();

            var mOutputs = outputs.ToManaged();
            var mErrors = (ManagedArray)errors;

            Debug.Assert(lowerWeights.Length != 0 && lowerWeights.Length == lowerErrors.Length);

            fixed (float* pOutputs = mOutputs.InternalArray, pErrors = mErrors.InternalArray)
            {
                var outputsPtr = mOutputs.ToPtr(pOutputs);
                var errorsPtr = mErrors.ToPtr(pErrors);

                if (function == ActivationFunction.Sigmoid)
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = 0.0f;
                        for (int lIdx = 0; lIdx < lowerErrors.Length; lIdx++)
                        {
                            var lowerWeightsMA = (ManagedArray2)lowerWeights[lIdx];
                            var lowerErrorsMA = (ManagedArray)lowerErrors[lIdx];

                            Debug.Assert(lowerWeightsMA.Size2 == lowerErrorsMA.Size);
                            Debug.Assert(lowerWeightsMA.Size1 == outputs.Size);

                            fixed (float* pLowerWeights = lowerWeightsMA.InternalArray, pLowerErrors = lowerErrorsMA.InternalArray)
                            {
                                sum += ComputeErrors_LowerErrorSum(lowerErrorsMA.ToPtr(pLowerErrors), lowerWeightsMA.ToPtr(pLowerWeights), oIdx);
                            }
                        }
                        errorsPtr[oIdx] = sum * SigmoidD(outputsPtr[oIdx], alpha);
                    }
                }
                else
                {
                    for (int oIdx = 0; oIdx < outputs.Size; oIdx++)
                    {
                        float sum = 0.0f;
                        for (int lIdx = 0; lIdx < lowerErrors.Length; lIdx++)
                        {
                            var lowerWeightsMA = (ManagedArray2)lowerWeights[lIdx];
                            var lowerErrorsMA = (ManagedArray)lowerErrors[lIdx];

                            Debug.Assert(lowerWeightsMA.Size2 == lowerErrorsMA.Size);
                            Debug.Assert(lowerWeightsMA.Size1 == outputs.Size);

                            fixed (float* plw = lowerWeightsMA.InternalArray, ple = lowerWeightsMA.InternalArray)
                            {
                                sum += ComputeErrors_LowerErrorSum(lowerErrorsMA.ToPtr(ple), lowerWeightsMA.ToPtr(plw), oIdx);
                            }
                        }
                        errorsPtr[oIdx] = sum * alpha;
                    }
                }
            }
        }

        public void ComputeGradientsFF(IDisposable state, Marshaled<DeviceArrayFactory[]> inputsM, Marshaled<IDeviceArray2[]> gradientsM, IDeviceArray biasGradients, Marshaled<IDeviceArray2[]> gradientSumsM, IDeviceArray biasGradientSums, IDeviceArray errors)
        {
            var gradients = gradientsM.Instance();
            var gradientSums = gradientSumsM.Instance();
            var inputs = inputsM.Instance();

            bool online = gradients != null && biasGradients != null;
            bool offline = gradientSums != null && biasGradientSums != null;

            var mErrors = (ManagedArray)errors;
            var mBiasGradients = (ManagedArray)biasGradients;
            var mBiasGradientSums = (ManagedArray)biasGradientSums;

            fixed (float* pErrors = mErrors.InternalArray, 
                pBiasGradients = online ? mBiasGradients.InternalArray : null, 
                pBiasGradientSums = offline ? mBiasGradientSums.InternalArray : null)
            {
                var errorsPtr = mErrors.ToPtr(pErrors);
                ManagedArrayPtr biasGradientsPtr = ManagedArrayPtr.Null;
                ManagedArrayPtr biasGradientSumsPtr = ManagedArrayPtr.Null;
                
                if (online) biasGradientsPtr = mBiasGradients.ToPtr(pBiasGradients);
                if (offline) biasGradientSumsPtr = mBiasGradientSums.ToPtr(pBiasGradientSums);

                for (int eIdx = 0; eIdx < errors.Size; eIdx++)
                {
                    if (online) biasGradientsPtr[eIdx] = errorsPtr[eIdx];
                    if (offline) biasGradientSumsPtr[eIdx] += errorsPtr[eIdx];

                    for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                    {
                        var inputsMA = (inputs[lIdx]()).ToManaged();
                        if (online && offline)
                        {
                            var gradientsMA = (ManagedArray2)gradients[lIdx];
                            var gradientSumsMA = (ManagedArray2)gradientSums[lIdx];
                            fixed (float* pi = inputsMA.InternalArray, pg = gradientsMA.InternalArray, pgs = gradientSumsMA.InternalArray)
                            {
                                ComputeGradients_SetAndAddGradients(inputsMA.ToPtr(pi), gradientsMA.ToPtr(pg), gradientSumsMA.ToPtr(pgs), errorsPtr, eIdx);
                            }
                        }
                        else if (online)
                        {
                            var gradientsMA = (ManagedArray2)gradients[lIdx];
                            fixed (float* pi = inputsMA.InternalArray, pg = gradientsMA.InternalArray)
                            {
                                ComputeGradients_SetGradients(inputsMA.ToPtr(pi), gradientsMA.ToPtr(pg), errorsPtr, eIdx);
                            }
                        }
                        else 
                        {
                            Debug.Assert(offline);
                            var gradientSumsMA = (ManagedArray2)gradientSums[lIdx];
                            fixed (float* pi = inputsMA.InternalArray, pgs = gradientSumsMA.InternalArray)
                            {
                                ComputeGradients_AddGradients(inputsMA.ToPtr(pi), gradientSumsMA.ToPtr(pgs), errorsPtr, eIdx);
                            }
                        }
                    }
                }
            }
        }

        public void ComputeGradientsBPTTPhase1(IDisposable state, Marshaled<DeviceArrayFactory[]> inputsM, Marshaled<IDeviceArray2[]> gradientsM, IDeviceArray biasGradients, IDeviceArray errors)
        {
            var gradients = gradientsM.Instance();
            var inputs = inputsM.Instance();

            Debug.Assert(gradients != null && biasGradients != null);

            var mErrors = (ManagedArray)errors;
            var mBiasGradients = (ManagedArray)biasGradients;

            fixed (float* pErrors = mErrors.InternalArray,
                pBiasGradients = mBiasGradients.InternalArray)
            {
                var errorsPtr = mErrors.ToPtr(pErrors);
                var biasGradientsPtr = mBiasGradients.ToPtr(pBiasGradients);

                for (int eIdx = 0; eIdx < errors.Size; eIdx++)
                {
                    biasGradientsPtr[eIdx] += errorsPtr[eIdx];

                    for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                    {
                        var inputsMA = (inputs[lIdx]()).ToManaged();
                        var gradientsMA = (ManagedArray2)gradients[lIdx];
                        fixed (float* pi = inputsMA.InternalArray, pg = gradientsMA.InternalArray)
                        {
                            ComputeGradients_AddGradients(inputsMA.ToPtr(pi), gradientsMA.ToPtr(pg), errorsPtr, eIdx);
                        }
                    }
                }
            }
        }

        public void ComputeGradientsBPTTPhase2(IDisposable state, Marshaled<DeviceArrayFactory[]> inputsM, Marshaled<IDeviceArray2[]> gradientsM, IDeviceArray biasGradients, Marshaled<IDeviceArray2[]> gradientSumsM, IDeviceArray biasGradientSums, IDeviceArray errors, int intItCount)
        {
            var gradients = gradientsM.Instance();
            var gradientSums = gradientSumsM.Instance();
            var inputs = inputsM.Instance();

            Debug.Assert(gradients != null && biasGradients != null);
            bool offline = gradientSums != null && biasGradientSums != null;
            float by = intItCount;

            var mErrors = (ManagedArray)errors;
            var mBiasGradients = (ManagedArray)biasGradients;
            var mBiasGradientSums = (ManagedArray)biasGradientSums;

            fixed (float* pErrors = mErrors.InternalArray,
                pBiasGradients = mBiasGradients.InternalArray,
                pBiasGradientSums = offline ? mBiasGradientSums.InternalArray : null)
            {
                var errorsPtr = mErrors.ToPtr(pErrors);
                ManagedArrayPtr biasGradientsPtr = ManagedArrayPtr.Null;
                ManagedArrayPtr biasGradientSumsPtr = ManagedArrayPtr.Null;

                biasGradientsPtr = mBiasGradients.ToPtr(pBiasGradients);
                if (offline) biasGradientSumsPtr = mBiasGradientSums.ToPtr(pBiasGradientSums);

                for (int eIdx = 0; eIdx < errors.Size; eIdx++)
                {
                    biasGradientsPtr[eIdx] += errorsPtr[eIdx];
                    biasGradientsPtr[eIdx] /= by;
                    if (offline) biasGradientSumsPtr[eIdx] += biasGradientsPtr[eIdx];

                    for (int lIdx = 0; lIdx < inputs.Length; lIdx++)
                    {
                        var inputsMA = (inputs[lIdx]()).ToManaged();
                        var gradientsMA = (ManagedArray2)gradients[lIdx];
                        if (offline)
                        {
                            var gradientSumsMA = (ManagedArray2)gradientSums[lIdx];
                            fixed (float* pi = inputsMA.InternalArray, pg = gradientsMA.InternalArray, pgs = gradientSumsMA.InternalArray)
                            {
                                ComputeGradients_AddDivAddGradients(inputsMA.ToPtr(pi), gradientsMA.ToPtr(pg), gradientSumsMA.ToPtr(pgs), errorsPtr, eIdx, by);
                            }
                        }
                        else
                        {
                            fixed (float* pi = inputsMA.InternalArray, pg = gradientsMA.InternalArray)
                            {
                                ComputeGradients_AddDivGradients(inputsMA.ToPtr(pi), gradientsMA.ToPtr(pg), errorsPtr, eIdx, by);
                            }
                        }
                    }
                };
            }
        }

        static float Sigmoid(float value, float alpha)
        {
            // return (2.0f / (1.0f + (float)Math.Exp(-alpha * value))) - 1.0f; // Logistics
            // return (float)Math.Tanh(value * alpha); // Tanh
            return (value * alpha) / (1 + Math.Abs(value * alpha)); // Elliot
        }

        static float SigmoidD(float value, float alpha) 
        {
            // return alpha * (1.0f - value * value) / 2.0f; // Logistics
            // return alpha * (1.0f - (value * value)); // Tanh
            return alpha * 1.0f / ((1.0f + Math.Abs(value * alpha)) * (1.0f + Math.Abs(value * alpha))); // Elliot
        }

        static float ComputeForward_Sum(ManagedArrayPtr inputs, ManagedArray2Ptr weights, int idx) 
        {
            float sum = 0.0f;
            for (int x = 0; x < inputs.Size; x++) sum += inputs[x] * weights[x, idx];
            return sum;
        }

        static float ComputeErrors_LowerErrorSum(ManagedArrayPtr lowerErrors, ManagedArray2Ptr lowerWeights, int idx) 
        {
            float sum = 0.0f;
            for (int x = 0; x < lowerErrors.Size; x++) sum += lowerErrors[x] * lowerWeights[idx, x];
            return sum;
        }

        static void ComputeGradients_SetGradients(ManagedArrayPtr inputs, ManagedArray2Ptr gradients, ManagedArrayPtr errors, int idx)
        {
            for (int x = 0; x < inputs.Size; x++) gradients[x, idx] = inputs[x] * errors[idx];
        }

        static void ComputeGradients_AddGradients(ManagedArrayPtr inputs, ManagedArray2Ptr gradients, ManagedArrayPtr errors, int idx)
        {
            for (int x = 0; x < inputs.Size; x++) gradients[x, idx] += inputs[x] * errors[idx];
        }

        static void ComputeGradients_SetAndAddGradients(ManagedArrayPtr inputs, ManagedArray2Ptr gradients, ManagedArray2Ptr gradientSums, ManagedArrayPtr errors, int idx)
        {
            for (int x = 0; x < inputs.Size; x++)
            {
                var v = inputs[x] * errors[idx];
                gradients[x, idx] = v;
                gradientSums[x, idx] += v;
            }
        }

        static void ComputeGradients_AddDivGradients(ManagedArrayPtr inputs, ManagedArray2Ptr gradients, ManagedArrayPtr errors, int idx, float by)
        {
            for (int x = 0; x < inputs.Size; x++)
            {
                gradients[x, idx] += inputs[x] * errors[idx];
                gradients[x, idx] /= by;
            }
        }

        static void ComputeGradients_AddDivAddGradients(ManagedArrayPtr inputs, ManagedArray2Ptr gradients, ManagedArray2Ptr gradientSums, ManagedArrayPtr errors, int idx, float by)
        {
            for (int x = 0; x < inputs.Size; x++)
            {
                gradients[x, idx] += inputs[x] * errors[idx];
                gradients[x, idx] /= by;
                gradientSums[x, idx] += gradients[x, idx];
            }
        }

        unsafe public void ComputeGradientsRTLR(Marshaled<RTLRLayerInfo[][]> inputLayerInfosM, Marshaled<RTLRComputationData> dataM, Marshaled<IDeviceArray[]> valueRelatedPBuffsM, IDeviceArray outputsA, IDeviceArray desiredOutputsA)
        {
            var data = dataM.Instance();
            var inputLayerInfos = inputLayerInfosM.Instance();

            var outputs = outputsA != null ? outputsA.ToManaged() : null;
            var desiredOutputs = desiredOutputsA != null ? desiredOutputsA.ToManaged() : null;
            var inputs = data.Inputs != null ? data.Inputs().ToManaged() : null;
            var valueRelatedPBuffs = valueRelatedPBuffsM.Instance();
            float gradient = 0.0f;

            fixed (float* pOutputs = outputs != null ? outputs.InternalArray : null,
                pDesiredOutputs = desiredOutputs != null ? desiredOutputs.InternalArray : null,
                pInputs = inputs != null ? inputs.InternalArray : null)
            {
                int outputLayerIndex = valueRelatedPBuffs.Length - 1;
                for (int kLayerIndex = 0; kLayerIndex < valueRelatedPBuffs.Length; kLayerIndex++)
                {
                    var layerNetValueDerivates = data.NetValueDerivates[kLayerIndex].ToManaged();
                    var p_i_j_k_Values = valueRelatedPBuffs[kLayerIndex].ToManaged();

                    bool computeGradient = kLayerIndex == outputLayerIndex && pOutputs != null && pDesiredOutputs != null;

                    fixed (float* pLayerNetValueDerivates = layerNetValueDerivates.InternalArray, pp_i_j_k_Values = p_i_j_k_Values.InternalArray)
                    {
                        var layerNetValueDerivatesPtr = layerNetValueDerivates.ToPtr(pLayerNetValueDerivates);
                        var p_i_j_k_ValuesPtr = p_i_j_k_Values.ToPtr(pp_i_j_k_Values);

                        for (int kValueIndex = 0; kValueIndex < p_i_j_k_Values.Size; kValueIndex++)
                        {
                            // i: iLayerIndex, iValueIndex
                            // j: jLayerIndex, jValueIndex
                            // k: kLayerIndex, kValueIndex

                            float netDeriv_k = layerNetValueDerivatesPtr[kValueIndex];
                            float sum = 0.0f;

                            var upperInfos_k = inputLayerInfos[kLayerIndex];
                            foreach (var upperNonInputLayerInfo in upperInfos_k)
                            {
                                Debug.Assert(upperNonInputLayerInfo.Weights != null);
                                int lLayerIndex = upperNonInputLayerInfo.Index;
                                var p_i_j_l_Values = valueRelatedPBuffs[lLayerIndex].ToManaged();
                                var weights = upperNonInputLayerInfo.Weights.ToManaged2();

                                Debug.Assert(p_i_j_l_Values.Size == weights.Size1);
                                Debug.Assert(weights.Size2 == p_i_j_k_Values.Size);

                                fixed (float* pp_i_j_l = p_i_j_l_Values.InternalArray, pWeights = weights.InternalArray)
                                {
                                    var p_i_j_l_ValuesPtr = p_i_j_l_Values.ToPtr(pp_i_j_l);
                                    var weightsPtr = weights.ToPtr(pWeights);

                                    for (int lValueIndex = 0; lValueIndex < p_i_j_l_Values.Size; lValueIndex++)
                                    {
                                        // i: iLayerIndex, iValueIndex
                                        // j: jLayerIndex, jValueIndex
                                        // k: kLayerIndex, kValueIndex
                                        // l: lLayerIndex, lValueIndex

                                        sum += weightsPtr[lValueIndex, kValueIndex] * p_i_j_l_ValuesPtr[lValueIndex];
                                    }
                                }
                            }

                            if (data.ILayerIndex == kLayerIndex && data.IValueIndex == kValueIndex)
                            {
                                if (inputs != null)
                                {
                                    // Weighted connection
                                    var inputsPtr = inputs.ToPtr(pInputs);
                                    sum += inputsPtr[data.JValueIndex];
                                }
                                else 
                                {
                                    Debug.Assert(data.JValueIndex == -1);

                                    // Biased connection
                                    sum += 1.0f;
                                }
                            }

                            p_i_j_k_ValuesPtr[kValueIndex] = netDeriv_k * sum;

                            if (computeGradient)
                            {
                                var outputsPtr = outputs.ToPtr(pOutputs);
                                var desiredOutputsPtr = desiredOutputs.ToPtr(pDesiredOutputs);
                                gradient += (desiredOutputsPtr[kValueIndex] - outputsPtr[kValueIndex]) * p_i_j_k_ValuesPtr[kValueIndex];
                            }
                        }
                    }
                }
            }

            if (gradient != 0.0f) SetGradientsRTLR(data, gradient);
        }

        unsafe private static void SetGradientsRTLR(RTLRComputationData data, float gradient)
        {
            var gradients = data.Gradients != null ? data.Gradients.ToManaged2() : null;
            var gradientSums = data.GradientSums != null ? data.GradientSums.ToManaged2() : null;
            var biasGradients = data.BiasGradients != null ? data.BiasGradients.ToManaged() : null;
            var biasGradientSums = data.BiasGradientSums != null ? data.BiasGradientSums.ToManaged() : null;

            fixed (float* pGradients = gradients != null ? gradients.InternalArray : null,
                pGradientSums = gradientSums != null ? gradientSums.InternalArray : null,
                pBiasGradients = biasGradients != null ? biasGradients.InternalArray : null,
                pBiasGradientSums = biasGradientSums != null ? biasGradientSums.InternalArray : null)
            {
                if (pGradients != null)
                {
                    Debug.Assert(data.JLayerIndex > 0);
                    var gradientsPtr = gradients.ToPtr(pGradients);
                    gradientsPtr[data.JValueIndex, data.IValueIndex] = gradient;
                }

                if (pGradientSums != null)
                {
                    Debug.Assert(data.JLayerIndex > 0);
                    var gradientSumsPtr = gradientSums.ToPtr(pGradientSums);
                    gradientSumsPtr[data.JValueIndex, data.IValueIndex] += gradient;
                }

                if (pBiasGradients != null)
                {
                    Debug.Assert(data.JLayerIndex == 0);
                    var biasGradientsPtr = biasGradients.ToPtr(pBiasGradients);
                    biasGradientsPtr[data.IValueIndex] = gradient;
                }

                if (pBiasGradientSums != null)
                {
                    Debug.Assert(data.JLayerIndex == 0);
                    var biasGradientSumsPtr = biasGradientSums.ToPtr(pBiasGradientSums);
                    biasGradientSumsPtr[data.IValueIndex] += gradient;
                }
            }
        }

        public void CalculateGlobalError(IDisposable state, IDeviceArray desiredOutputsA, IDeviceArray actualOutputsA, IDeviceArray errorValueA, IDeviceArray errorSumValueA)
        {
            var actualOutputs = actualOutputsA.ToManaged();
            var desiredOutputs = desiredOutputsA.ToManaged();
            var errorValue = errorValueA.ToManaged();
            var errorSumValue = errorSumValueA != null ? errorSumValueA.ToManaged() : null;
            fixed (float* pActualOutputs = actualOutputs.InternalArray, 
                pDesiredOutputs = desiredOutputs.InternalArray,
                pErrorValue = errorValue.InternalArray,
                pErrorSumValue = errorSumValue != null ? errorSumValue.InternalArray : null)
            {
                var actualOutputsPtr = actualOutputs.ToPtr(pActualOutputs);
                var desiredOutputsPtr = desiredOutputs.ToPtr(pDesiredOutputs);
                var errorValuePtr = errorValue.ToPtr(pErrorValue);
                float cMse = 0.0f;

                for (int x = 0; x < actualOutputs.Size; x++)
                {
                    float error = (desiredOutputsPtr[x] - actualOutputsPtr[x]) * 0.5f;
                    cMse += error * error;
                }
                errorValuePtr[0] = cMse / (float)actualOutputs.Size;
                if (pErrorSumValue != null)
                {
                    var errorSumValuePtr = errorSumValue.ToPtr(pErrorSumValue);
                    errorSumValuePtr[0] += errorValuePtr[0];
                }
            }
        }
    }
}
