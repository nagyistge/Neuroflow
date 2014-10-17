using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural
{
    public class ConnectedLayerGroups
    {
        #region The Connector class
        
        class Connector
        {
            struct ConnectionKey : IEquatable<ConnectionKey>
            {
                public ConnectionKey(Guid uid1, Guid uid2)
                {
                    this.uid1 = uid1;
                    this.uid2 = uid2;
                }

                Guid uid1, uid2;

                public bool Equals(ConnectionKey other)
                {
                    return other.uid1 == uid1 && other.uid2 == uid2;
                }

                public override int GetHashCode()
                {
                    return uid1.GetHashCode() ^ uid2.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return obj is ConnectionKey ? Equals((ConnectionKey)obj) : false;
                }
            }

            internal Connector(BufferAllocator allocator, NNStructuralElement seFlags, RecurrentOptions recOptions)
            {
                Contract.Requires(allocator != null);

                this.allocator = allocator;
                this.seFlags = seFlags;
                this.recOptions = recOptions;
            }

            RecurrentOptions recOptions;

            NNStructuralElement seFlags;

            Dictionary<Guid, IntRange> layerValueBuffers;

            Dictionary<Guid, IntRange> layerErrorBuffers;

            Dictionary<Guid, IntRange> layerNetDerivBuffers;

            Dictionary<ConnectionKey, WeightedValueBuffer> layerWeightedValueBuffers;

            BufferAllocator allocator;

            internal ConnectedLayer Connect(ConnectableLayer layer, bool layerIsOutput)
            {
                Contract.Requires(layer != null);

                IEnumerable<ConnectableLayer> upperLayers, lowerLayers;
                if (recOptions == null)
                {
                    upperLayers = layer.UpperLayers;
                    lowerLayers = layer.LowerLayers;
                }
                else
                {
                    var layerEnum = Enumerable.Repeat(layer, recOptions.IsFullRecurrent ? 1 : 0);
                    upperLayers = layer.UpperLayers.Concat(layer.LowerLayers).Concat(layerEnum).ToList();
                    lowerLayers = layer.LowerLayers.Concat(layer.UpperLayers.Where(l => !(l.Layer is InputLayer))).Concat(layerEnum).ToList();
                }

                // Forward
                int? biasIdx = null;
                if (layer.IsBiased) biasIdx = allocator.Alloc(1).MinValue;

                var output = GetLayerValueBuffer(layer);

                var wiBuffs = CreateWeightedInputBuffers(layer, upperLayers);

                if (seFlags == NNStructuralElement.None)
                {
                    return new ConnectedLayer(layer.Index, layer.Layer, seFlags)
                    {
                        BiasValueIndex = biasIdx,
                        WeightedInputBuffers = wiBuffs,
                        OutputBuffer = output,
                        IsOutput = layerIsOutput
                    };
                }

                // Gradient:
                int? biasGIndex = null;
                int? biasGSindex = null;
                IntRange[] gradBuffs = null;
                IntRange[] gradSumBuffs = null;
                if ((seFlags & NNStructuralElement.GradientInformation) != 0)
                {
                    if (layer.IsBiased)
                    {
                        biasGIndex = allocator.Alloc(1).MinValue;
                        biasGSindex = allocator.Alloc(1).MinValue;
                    }
                    gradBuffs = CreateLayerGradientBuffers(allocator, layer, upperLayers);
                    gradSumBuffs = CreateLayerGradientSumBuffers(allocator, layer, upperLayers);
                }

                // Backward:
                IntRange? errorBuff = null;
                WeightedValueBuffer[] woeBuffs = null;
                IntRange[][] innerItarationInputValueStacks = null;
                IntRange[] innerItarationOutputValueStack = null;
                IntRange? outputErrorBuffer = null;
                if ((seFlags & NNStructuralElement.GradientInformation) != 0)
                {
                    bool isBPTT = recOptions != null && recOptions.Algorithm == RLAlgorithm.BPTT;

                    errorBuff = GetLayerErrorBuffer(layer);
                    woeBuffs = CreateWeightedLowerErrorBuffers(layer, lowerLayers);

                    if (layerIsOutput)
                    {
                        outputErrorBuffer = allocator.Alloc(layer.Size);
                    }

                    if (isBPTT)
                    {
                        // For inputs:
                        innerItarationInputValueStacks = new IntRange[wiBuffs.Length][];
                        for (int inputBufferIndex = 0; inputBufferIndex < wiBuffs.Length; inputBufferIndex++)
                        {
                            var wiBuff = wiBuffs[inputBufferIndex];
                            innerItarationInputValueStacks[inputBufferIndex] = new IntRange[wiBuff.ValueBuffer.Size];
                            for (int inputIndex = 0; inputIndex < wiBuff.ValueBuffer.Size; inputIndex++)
                            {
                                innerItarationInputValueStacks[inputBufferIndex][inputIndex] = allocator.Alloc(recOptions.MaxIterations);
                            }
                        }

                        //For values:
                        innerItarationOutputValueStack = new IntRange[output.Size];
                        for (int i = 0; i < innerItarationOutputValueStack.Length; i++) innerItarationOutputValueStack[i] = allocator.Alloc(recOptions.MaxIterations);
                    }
                }

                // RTLR
                UpperLayerInfo[] upperNonInputLayerInfos = null;
                IntRange? netDerivBuff = null;
                if ((seFlags & NNStructuralElement.RTLRInformation) != 0)
                {
                    upperNonInputLayerInfos = (from i in upperLayers.Select((l, i) => new { Layer = l, LocalIndex = i })
                                               let upperLayer = i.Layer
                                               let buffIndex = i.LocalIndex
                                               where !(upperLayer.Layer is InputLayer)
                                               select new UpperLayerInfo(buffIndex, upperLayer.Index)).ToArray();

                    netDerivBuff = GetLayerNetDerivBuffer(layer);
                }

                return new ConnectedLayer(layer.Index, layer.Layer, seFlags)
                {
                    // Forward:
                    BiasValueIndex = biasIdx,
                    WeightedInputBuffers = wiBuffs,
                    OutputBuffer = output,
                    IsOutput = layerIsOutput,
                    // Gradient:
                    BiasGradientValueIndex = biasGIndex,
                    BiasGradientSumValueIndex = biasGSindex,
                    GradientBuffers = gradBuffs,
                    GradientSumBuffers = gradSumBuffs,
                    // Backward:
                    ErrorBuffer = errorBuff,
                    WeightedOutputErrorBuffers = woeBuffs,
                    OutputErrorBuffer = outputErrorBuffer,
                    InnerItarationInputValueStacks = innerItarationInputValueStacks,
                    InnerItarationOutputValueStack = innerItarationOutputValueStack,
                    // RTLR:
                    UpperNonInputLayerInfos = upperNonInputLayerInfos,
                    NetDerivBuffer = netDerivBuff,
                };
            }

            private WeightedValueBuffer[] CreateWeightedInputBuffers(ConnectableLayer layer, IEnumerable<ConnectableLayer> upperLayers)
            {
                return upperLayers.Select(upperLayer => GetWeightedValueBuffer(upperLayer, layer)).ToArray();
            }

            private IntRange[] CreateLayerGradientBuffers(BufferAllocator allocator, ConnectableLayer layer, IEnumerable<ConnectableLayer> upperLayers)
            {
                return upperLayers.Select(upperLayer => allocator.Alloc(upperLayer.Size * layer.Size)).ToArray();
            }

            private IntRange[] CreateLayerGradientSumBuffers(BufferAllocator allocator, ConnectableLayer layer, IEnumerable<ConnectableLayer> upperLayers)
            {
                return upperLayers.Select(upperLayer => allocator.Alloc(upperLayer.Size * layer.Size)).ToArray();
            }

            private WeightedValueBuffer[] CreateWeightedLowerErrorBuffers(ConnectableLayer layer, IEnumerable<ConnectableLayer> lowerLayers)
            {
                return lowerLayers
                    .Select(
                        lowerLayer =>
                        {
                            var weightedInputsOfLowers = GetWeightedValueBuffer(layer, lowerLayer);
                            var errorsOfLower = GetLayerErrorBuffer(lowerLayer);
                            return new WeightedValueBuffer(errorsOfLower, weightedInputsOfLowers.WeightBuffer);
                        })
                    .ToArray();
            }

            internal IntRange GetLayerValueBuffer(ConnectableLayer layer)
            {
                return AllocateLayerBuffer(layer, ref layerValueBuffers);
            }

            private IntRange GetLayerErrorBuffer(ConnectableLayer layer)
            {
                return AllocateLayerBuffer(layer, ref layerErrorBuffers);
            }

            private IntRange GetLayerNetDerivBuffer(ConnectableLayer layer)
            {
                return AllocateLayerBuffer(layer, ref layerNetDerivBuffers);
            }

            private WeightedValueBuffer GetWeightedValueBuffer(ConnectableLayer upperLayer, ConnectableLayer lowerLayer)
            {
                var key = new ConnectionKey(upperLayer.UID, lowerLayer.UID);
                WeightedValueBuffer result;
                bool isNew = false;

                if (layerWeightedValueBuffers == null)
                {
                    layerWeightedValueBuffers = new Dictionary<ConnectionKey, WeightedValueBuffer>();
                    isNew = true;
                }

                if (!isNew && layerWeightedValueBuffers.TryGetValue(key, out result)) return result;

                var input = GetLayerValueBuffer(upperLayer);
                var weights = allocator.Alloc(input.Size * lowerLayer.Size);
                result = new WeightedValueBuffer(input, weights);
                layerWeightedValueBuffers.Add(key, result);
                return result;
            }

            private IntRange AllocateLayerBuffer(ConnectableLayer layer, ref Dictionary<Guid, IntRange> registry)
            {
                IntRange result;
                bool isNew = false;

                if (registry == null)
                {
                    registry = new Dictionary<Guid, IntRange>();
                    isNew = true;
                }

                if (!isNew && registry.TryGetValue(layer.UID, out result)) return result;

                result = allocator.Alloc(layer.Size);
                registry.Add(layer.UID, result);
                return result;
            }
        } 

        #endregion

        public ConnectedLayerGroups(BufferAllocator allocator, GroupedLayers groupedLayers, NNStructuralElement seFlags, RecurrentOptions recOptions)
        {
            Contract.Requires(allocator != null);
            Contract.Requires(groupedLayers != null);

            Build(allocator, groupedLayers, recOptions, seFlags);
        }

        public IntRange InputBuffer { get; private set; }

        public IntRange OutputBuffer { get; private set; }

        public ReadOnlyCollection<ReadOnlyCollection<ConnectedLayer>> Groups { get; private set; }

        public UnitIndexTable IndexTable { get; private set; }

        private void Build(BufferAllocator allocator, GroupedLayers groupedLayers, RecurrentOptions recOptions, NNStructuralElement seFlags)
        {
            var connector = new Connector(allocator, seFlags, recOptions);
            var connectedGroups = new List<ReadOnlyCollection<ConnectedLayer>>(groupedLayers.LayerGroups.Count - 1);

            InitLayerIndexes(groupedLayers);

            for (int groupIndex = 0; groupIndex < groupedLayers.LayerGroups.Count - 1; groupIndex++)
            {
                var group = groupedLayers.LayerGroups[groupIndex + 1];
                var connectedLayers = new List<ConnectedLayer>(group.Count);

                for (int layerIndex = 0; layerIndex < group.Count; layerIndex++)
                {
                    var layer = group[layerIndex];
                    connectedLayers.Add(connector.Connect(layer, layer == groupedLayers.OutputLayer));
                }

                connectedGroups.Add(connectedLayers.AsReadOnly());
            }

            Groups = connectedGroups.AsReadOnly();
            InputBuffer = connector.GetLayerValueBuffer(groupedLayers.InputLayer);
            OutputBuffer = connector.GetLayerValueBuffer(groupedLayers.OutputLayer);
            IndexTable = new UnitIndexTable(groupedLayers.InputLayer.Size, Groups.SelectMany(g => g).Select(l => l.Layer.Size));

            if ((seFlags & NNStructuralElement.RTLRInformation) != 0)
            {
                Debug.Assert(recOptions != null && recOptions.Algorithm == RLAlgorithm.RTLR);

                BuildRTLR(allocator);
            }
        }

        private static void InitLayerIndexes(GroupedLayers groupedLayers)
        {
            int clIndex = 0;
            for (int groupIndex = 0; groupIndex < groupedLayers.LayerGroups.Count - 1; groupIndex++)
            {
                var group = groupedLayers.LayerGroups[groupIndex + 1];
                for (int layerIndex = 0; layerIndex < group.Count; layerIndex++)
                {
                    var layer = group[layerIndex];
                    layer.Index = clIndex++;
                }
            }
        }

        private void BuildRTLR(BufferAllocator allocator)
        {
            var connectedLayers = Groups.SelectMany(g => g);
            foreach (var layer in connectedLayers)
            {
                BuildRTLR(allocator, layer);
            }
        }

        private void BuildRTLR(BufferAllocator allocator, ConnectedLayer layer)
        {
            // P for Bias
            IntRange[] pBiasBuffs = null;
            IntRange[] prevPBiasBuffs = null;
            if (layer.IsBiased)
            {
                pBiasBuffs = new IntRange[IndexTable.OtherLayerSizes.Length];
                prevPBiasBuffs = new IntRange[IndexTable.OtherLayerSizes.Length];
                for (int otherLayerIdx = 0; otherLayerIdx < IndexTable.OtherLayerSizes.Length; otherLayerIdx++)
                {
                    pBiasBuffs[otherLayerIdx] = allocator.Alloc(IndexTable.OtherLayerSizes[otherLayerIdx]);
                    prevPBiasBuffs[otherLayerIdx] = allocator.Alloc(IndexTable.OtherLayerSizes[otherLayerIdx]);
                }
            }

            // P for Weights
            IntRange[][][] pWeightBuffs = new IntRange[layer.WeightedInputBuffers.Length][][];
            IntRange[][][] prevPWeightBuffs = new IntRange[layer.WeightedInputBuffers.Length][][];
            for (int wiBuffIdx = 0; wiBuffIdx < layer.WeightedInputBuffers.Length; wiBuffIdx++)
            {
                var wiBuff = layer.WeightedInputBuffers[wiBuffIdx];
                int wiBuffSize = wiBuff.WeightBuffer.Size;
                pWeightBuffs[wiBuffIdx] = new IntRange[wiBuffSize][];
                prevPWeightBuffs[wiBuffIdx] = new IntRange[wiBuffSize][];
                for (int weightIdx = 0; weightIdx < wiBuffSize; weightIdx++)
                {
                    pWeightBuffs[wiBuffIdx][weightIdx] = new IntRange[IndexTable.OtherLayerSizes.Length];
                    prevPWeightBuffs[wiBuffIdx][weightIdx] = new IntRange[IndexTable.OtherLayerSizes.Length];
                    for (int otherLayerIdx = 0; otherLayerIdx < IndexTable.OtherLayerSizes.Length; otherLayerIdx++)
                    {
                        pWeightBuffs[wiBuffIdx][weightIdx][otherLayerIdx] = allocator.Alloc(IndexTable.OtherLayerSizes[otherLayerIdx]);
                        prevPWeightBuffs[wiBuffIdx][weightIdx][otherLayerIdx] = allocator.Alloc(IndexTable.OtherLayerSizes[otherLayerIdx]);
                    }
                }
            }

            layer.PBiasBuffers = pBiasBuffs;
            layer.PrevPBiasBuffers = prevPBiasBuffs;
            layer.PWeightBuffers = pWeightBuffs;
            layer.PrevPWeightBuffers = prevPWeightBuffs;
        }
    }
}
