﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    /// <summary>
    /// http://www.willamette.edu/~gorr/classes/cs449/rtrl.html
    /// </summary>
    internal sealed class RTLR : DisposableObject
    {
        internal RTLR(MultilayerPerceptron mlp)
        {
            Debug.Assert(mlp != null);

            this.mlp = mlp;
            this.netValueDerivates = mlp.NetValueDerivates.Values.ToArray();

            CreatePValues(mlp);
        }

        MultilayerPerceptron mlp;

        List<Marshaled<IDeviceArray[]>[][]> pWeightValues = new List<Marshaled<IDeviceArray[]>[][]>();

        List<Action<Marshaled<IDeviceArray[]>, IDeviceArray, IDeviceArray>> codes = new List<Action<Marshaled<IDeviceArray[]>, IDeviceArray, IDeviceArray>>();

        IDeviceArray[] netValueDerivates;

        private void CreatePValues(MultilayerPerceptron mlp)
        {
            int uLayersCount = mlp.Layers.Count - 1;
            for (int lidx = 1; lidx < mlp.Layers.Count; lidx++)
            {
                var layer = mlp.Layers[lidx];
                var biases = mlp.Biases[lidx];

                var pWeightValues = new LinkedList<Marshaled<IDeviceArray[]>[]>();

                //// Biases:
                var pWeightValuesOfInput = new Marshaled<IDeviceArray[]>[biases.Size];

                for (int weightIndex = 0; weightIndex < biases.Size; weightIndex++)
                {
                    pWeightValuesOfInput[weightIndex] = mlp.AsMarshaled(new IDeviceArray[uLayersCount]);

                    for (int lidx2 = 0; lidx2 < uLayersCount; lidx2++)
                    {
                        pWeightValuesOfInput[weightIndex].Instance()[lidx2] = mlp.Adapter.DeviceArrayManagement.CreateArray(false, GetULayerSize(lidx2));
                    }
                }

                pWeightValues.AddLast(pWeightValuesOfInput);

                // Weighted conns:
                foreach (var inputConnectedLayer in layer.Layer.GetInputLayers())
                {
                    int inputIndex = mlp.GetLayerIndex(inputConnectedLayer);
                    var key = Tuple.Create(inputIndex, lidx);
                    var weigths = mlp.Weights[key];

                    pWeightValuesOfInput = new Marshaled<IDeviceArray[]>[weigths.Size];

                    for (int weightIndex = 0; weightIndex < weigths.Size; weightIndex++)
                    {
                        pWeightValuesOfInput[weightIndex] = mlp.AsMarshaled(new IDeviceArray[uLayersCount]);
                        for (int lidx2 = 0; lidx2 < uLayersCount; lidx2++)
                        {
                            pWeightValuesOfInput[weightIndex].Instance()[lidx2] = mlp.Adapter.DeviceArrayManagement.CreateArray(false, GetULayerSize(lidx2));
                        }
                    }

                    pWeightValues.AddLast(pWeightValuesOfInput);
                }

                this.pWeightValues.Add(pWeightValues.ToArray());
            }
        }

        internal void ComputeGradients(IDeviceArray desiredOutputs)
        {
            int computationIndex = 0;
            var outputs = mlp.GetNetValues(mlp.Layers.Count - 1);

            for (int lidx = 1; lidx < mlp.Layers.Count; lidx++)
            {
                var layer = mlp.Layers[lidx];
                int layerOutputBufferSize = layer.Layer.Size;
                int iLayerIndex = lidx - 1;

                var layerPWeightValues = GetPWeights(iLayerIndex);

                for (int iValueIndex = 0; iValueIndex < layerOutputBufferSize; iValueIndex++)
                {
                    // i: iLayerIndex, iValueIndex

                    for (int jLayerIndex = 0; jLayerIndex < layerPWeightValues.Length; jLayerIndex++)
                    {
                        var layerRelatedPWeightBuffs = layerPWeightValues[jLayerIndex];

                        if (jLayerIndex == 0)
                        {
                            // Biased connections

                            var valueRelatedPBuffs = layerRelatedPWeightBuffs[iValueIndex];

                            // i: iLayerIndex, iValueIndex
                            // j: jLayerIndex, jValueIndex

                            ComputeGradients(computationIndex++, valueRelatedPBuffs, iLayerIndex, iValueIndex, jLayerIndex, -1, outputs, desiredOutputs);
                        }
                        else
                        {
                            // Weighted connections
                            int inputLayerSize = layerRelatedPWeightBuffs.Length / layerOutputBufferSize;

                            Debug.Assert(layerOutputBufferSize * inputLayerSize == layerRelatedPWeightBuffs.Length);

                            for (int jValueIndex = 0; jValueIndex < inputLayerSize; jValueIndex++)
                            {
                                int ijValueIndex = GetWeightValueIndex(jValueIndex, iValueIndex, inputLayerSize);

                                var valueRelatedPBuffs = layerRelatedPWeightBuffs[ijValueIndex];

                                // i: iLayerIndex, iValueIndex
                                // j: jLayerIndex, jValueIndex

                                ComputeGradients(computationIndex++, valueRelatedPBuffs, iLayerIndex, iValueIndex, jLayerIndex, jValueIndex, outputs, desiredOutputs);
                            }
                        }
                    }
                }
            }
        }

        private void ComputeGradients(int computationIndex, Marshaled<IDeviceArray[]> valueRelatedPBuffs, int iLayerIndex, int iValueIndex, int jLayerIndex, int jValueIndex, IDeviceArray outputs, IDeviceArray desiredOutputs)
        {
#if DEBUG
            int outputLayerIndex = valueRelatedPBuffs.Instance().Length - 1;
            Debug.Assert(outputLayerIndex == mlp.Layers.Count - 2);
            Debug.Assert(outputLayerIndex == mlp.Layers[mlp.Layers.Count - 1].Index - 1);
#endif

            if (codes.Count > computationIndex)
            {
                var code = codes[computationIndex];
                if (code != null) code(valueRelatedPBuffs, outputs, desiredOutputs);
            }
            else
            {
                codes.EnsureSize(computationIndex + 1);
                Action<Marshaled<IDeviceArray[]>, IDeviceArray, IDeviceArray> code = null;

                bool forBias = jValueIndex == -1;
                var dataM = mlp.AsMarshaled(new RTLRComputationData());
                var data = dataM.ManagedObject;

                int iLayerIndexN = iLayerIndex + 1;
                var iLayer = mlp.Layers[iLayerIndexN];
                data.ILayerIndex = iLayerIndex;
                data.IValueIndex = iValueIndex;
                data.JLayerIndex = jLayerIndex;
                data.JValueIndex = jValueIndex;
                if (forBias)
                {
                    Debug.Assert(jLayerIndex == 0);
                    data.BiasGradients = mlp.GetBiasGradients(iLayerIndexN);
                    data.BiasGradientSums = mlp.GetBiasGradientSums(iLayerIndexN);
                }
                else
                {
                    Debug.Assert(jLayerIndex > 0);
                    var inputLayerOfILayer = iLayer.Layer.GetInputLayer(jLayerIndex - 1);
                    var inputLayerOfILayerIndex = mlp.GetLayerIndex(inputLayerOfILayer);
                    var weightKey = Tuple.Create(inputLayerOfILayerIndex, iLayerIndexN);
                    data.Inputs = () => mlp.GetNetValues(inputLayerOfILayerIndex);
                    data.Gradients = mlp.GetGradients(weightKey);
                    data.GradientSums = mlp.GetGradientSums(weightKey);
                }

                Debug.Assert(!(data.BiasGradients == null && data.BiasGradientSums == null && data.Gradients == null && data.GradientSums == null));

                data.NetValueDerivates = netValueDerivates;
                data.InputLayerInfos =
                    (from lidx in Enumerable.Range(1, mlp.Layers.Count - 1)
                     let layer = mlp.Layers[lidx].Layer
                     select (from inputLayer in layer.GetInputLayers()
                             where inputLayer != mlp.Layers[0].Layer
                             let iidx = mlp.GetLayerIndex(inputLayer)
                             select new RTLRLayerInfo
                             {
                                 Index = iidx - 1,
                                 Size = inputLayer.Size,
                                 Weights = mlp.Weights[Tuple.Create(iidx, lidx)]
                             }).ToArray()).ToArray();

                code = (p, os, dos) => mlp.Adapter.ComputeActivation.ComputeGradientsRTLR(dataM, p, os, dos);

                codes[computationIndex] = code;
                code(valueRelatedPBuffs, outputs, desiredOutputs);
            }
        }

        private Marshaled<IDeviceArray[]>[][] GetPWeights(int iLayerIndex)
        {
            return pWeightValues[iLayerIndex];
        }

        static int GetWeightValueIndex(int upperValueIndex, int lowerValueIndex, int lowerLayerSize)
        {
            return lowerValueIndex * lowerLayerSize + upperValueIndex;
        }       

        private int GetULayerSize(int lidx)
        {
            return mlp.Layers[lidx + 1].Layer.Size;
        }

        #region Reset

        internal void Reset()
        {
            foreach (var p in pWeightValues) Reset(p);
        }

        private void Reset(Marshaled<IDeviceArray[]>[][] p)
        {
            foreach (var ip in p) Reset(ip);
        }

        private void Reset(Marshaled<IDeviceArray[]>[] p)
        {
            foreach (var ip in p) Reset(ip);
        }

        private void Reset(Marshaled<IDeviceArray[]> p)
        {
            foreach (var da in p.Instance()) mlp.Adapter.VectorUtils.Zero(da);
        }

        #endregion

        #region Free

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Free();
        }

        private void Free()
        {
            if (pWeightValues != null)
            {
                foreach (var p in pWeightValues) Free(p);
                pWeightValues = null;
            }
        }

        private void Free(Marshaled<IDeviceArray[]>[][] p)
        {
            foreach (var ip in p) Free(ip);
        }

        private void Free(Marshaled<IDeviceArray[]>[] p)
        {
            foreach (var ip in p) Free(ip);
        }

        private void Free(Marshaled<IDeviceArray[]> p)
        {
            ResourceManager.Free(p.Instance());
        }

        #endregion
    }
}
