using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    internal sealed class RTLR2 : DisposableObject
    {
        internal RTLR2(MultilayerPerceptron mlp)
        {
            Debug.Assert(mlp != null);

            this.mlp = mlp;
            this.netValueDerivates = mlp.AsMarshaled(mlp.NetValueDerivates.Values.ToArray());
            this.inputLayerInfos = mlp.AsMarshaled(
                (from lidx in Enumerable.Range(1, mlp.Layers.Count - 1)
                 let layer = mlp.Layers[lidx].Layer
                 select (from inputLayer in layer.GetInputLayers()
                         let iidx = mlp.GetLayerIndex(inputLayer)
                         select new RTLRLayerInfo
                         {
                             Index = iidx - 1,
                             Size = inputLayer.Size,
                             Weights = mlp.Weights[Tuple.Create(iidx, lidx)],
                             IsElementOfU = inputLayer != mlp.Layers[0].Layer
                         }).ToArray()).ToArray());

            CreatePValues();
        }

        MultilayerPerceptron mlp;

        int uLayersCount;

        int maxULayerSize;

        Marshaled<RTLRLayerInfo[][]> inputLayerInfos;

        Marshaled<IDeviceArray[]> netValueDerivates;

        IDeviceArrayPool pValuesPool;

        IDeviceArray2[][] pValues;

        List<Action<IDeviceArray, IDeviceArray>> codes = new List<Action<IDeviceArray, IDeviceArray>>();

        private void CreatePValues()
        {
            var pvs = new LinkedList<IDeviceArray2[]>();
            pValuesPool = mlp.Adapter.DeviceArrayManagement.CreatePool();
            uLayersCount = mlp.Layers.Count - 1;
            maxULayerSize = mlp.Layers.Where(l => l.Index != 0).Max(l => l.Layer.Size);
            for (int lidx = 1; lidx < mlp.Layers.Count; lidx++)
            {
                var lpvs = new LinkedList<IDeviceArray2>();
                var layer = mlp.Layers[lidx];
                var biases = mlp.Biases[lidx];
                lpvs.AddLast(CreatePValuesForWeights(biases));
                foreach (var inputConnectedLayer in layer.Layer.GetInputLayers())
                {
                    int inputIndex = mlp.GetLayerIndex(inputConnectedLayer);
                    var wkey = Tuple.Create(inputIndex, lidx);
                    var weigths = mlp.Weights[wkey];
                    lpvs.AddLast(CreatePValuesForWeights(weigths));
                }
                pvs.AddLast(lpvs.ToArray());
            }
            pValues = pvs.ToArray();
        }

        private IDeviceArray2 CreatePValuesForWeights(IDeviceArray weights)
        {
            int xSize = uLayersCount * maxULayerSize;
            int ySize = weights.Size;
            return pValuesPool.CreateArray2(ySize, xSize);
        }

        internal void ComputeGradients(IDeviceArray desiredOutputs)
        {
            var outputs = mlp.GetNetValues(mlp.Layers.Count - 1);
            int computationIndex = 0;
            for (int lidx = 1; lidx < mlp.Layers.Count; lidx++)
            {
                int iLayerIndex = lidx - 1;
                var pValuesOfLayer = pValues[iLayerIndex];
                for (int jLayerIndex = 0; jLayerIndex < pValuesOfLayer.Length; jLayerIndex++)
                {
                    // 0: Bias
                    // 1..: Weights
                    var pValuesOfWeights = pValuesOfLayer[jLayerIndex];
                    SequenceMarker seqMark = SequenceMarker.Inner;
                    if (lidx == 1 && jLayerIndex == 0) seqMark = SequenceMarker.Begin;
                    else if (lidx == mlp.Layers.Count - 1 && jLayerIndex == pValuesOfLayer.Length - 1) seqMark = SequenceMarker.End;
                    ComputeGradients(iLayerIndex, jLayerIndex, pValuesOfWeights, outputs, desiredOutputs, computationIndex++, seqMark);
                }
            }
        }

        private void ComputeGradients(int iLayerIndex, int jLayerIndex, IDeviceArray2 pValuesOfWeights, IDeviceArray outputs, IDeviceArray desiredOutputs, int computationIndex, SequenceMarker seqMark)
        {
            // jLayerIndex: 0: Bias, 1..: Weights

            if (codes.Count > computationIndex)
            {
                var code = codes[computationIndex];
                if (code != null) code(outputs, desiredOutputs);
            }
            else
            {
                codes.EnsureSize(computationIndex + 1);
                Action<IDeviceArray, IDeviceArray> code = null;

                bool forBias = jLayerIndex == 0;
                var dataM = mlp.AsMarshaled(new RTLRComputationData2());
                var data = dataM.ManagedObject;
                data.MaxULayerSize = maxULayerSize;
                data.ULayersCount = uLayersCount;
                int iLayerIndexN = iLayerIndex + 1;
                var iLayer = mlp.Layers[iLayerIndexN];
                data.ILayerIndex = iLayerIndex;
                data.JLayerIndex = jLayerIndex;
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

                var state = mlp.CreateComputationState();
                code = (os, dos) => mlp.Adapter.ComputeActivation.ComputeGradientsRTLR2(state, inputLayerInfos, netValueDerivates, dataM, pValuesOfWeights, os, dos, seqMark);

                codes[computationIndex] = code;
                code(outputs, desiredOutputs);
            }
        }

        #region Zero

        internal void Zero()
        {
            pValuesPool.Zero();
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Free();
        }

        private void Free()
        {
            if (pValuesPool != null)
            {
                ResourceManager.Free(pValuesPool);
                foreach (var p in pValues) ResourceManager.Free(p);
                pValues = null;
                pValuesPool = null;
            }
        } 

        #endregion
    }
}
