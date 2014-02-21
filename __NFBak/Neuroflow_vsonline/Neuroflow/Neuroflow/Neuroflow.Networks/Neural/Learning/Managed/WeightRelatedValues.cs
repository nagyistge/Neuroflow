using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public sealed class WeightRelatedValues
    {
        public WeightRelatedValues(BufferAllocator allocator, ConnectedLayer[] layers)
        {
            Contract.Requires(allocator != null);
            Contract.Requires(layers != null);
            Contract.Requires(layers.Length > 0);

            buffers = new IntRange[layers.Length][];
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var wib = layers[layerIndex].WeightedInputBuffers;

                buffers[layerIndex] = new IntRange[wib.Length];
                for (int buffIndex = 0; buffIndex < wib.Length; buffIndex++)
                {
                    buffers[layerIndex][buffIndex] = allocator.Alloc(wib[buffIndex].WeightBuffer.Size);
                }
            }
            this.layers = layers;
        }

        ConnectedLayer[] layers;

        IntRange[][] buffers;

        public IntRange GetBuffer(int layerIndex, int bufferIndex)
        {
            return buffers[layerIndex][bufferIndex];
        }

        public unsafe void Fill(float* valueBuffer, float value)
        {
            foreach (var b in buffers)
                foreach (var range in b)
                    for (int i = range.MinValue; i <= range.MaxValue; i++)
                        valueBuffer[i] = value;
        }

        public unsafe void Zero(float* valueBuffer)
        {
            Fill(valueBuffer, 0);
        }

        public unsafe void SaveWeights(float* valueBuffer)
        {
            Contract.Requires(valueBuffer != null);

            for (int layerIndex = 0; layerIndex < buffers.Length; layerIndex++)
            {
                var layerBuffers = buffers[layerIndex];
                var layer = layers[layerIndex];

                Debug.Assert(layerBuffers.Length == layer.WeightedInputBuffers.Length);

                for (int bufferIndex = 0; bufferIndex < layerBuffers.Length; bufferIndex++)
                {
                    var buffRange = layerBuffers[bufferIndex];
                    var weightRange = layer.WeightedInputBuffers[bufferIndex].WeightBuffer;

                    Debug.Assert(buffRange.Size == weightRange.Size);

                    for (int buffIndex = buffRange.MinValue, weightIndex = weightRange.MinValue; buffIndex <= buffRange.MaxValue; buffIndex++, weightIndex++)
                    {
                        valueBuffer[buffIndex] = valueBuffer[weightIndex];
                    }
                }
            }
        }

        public unsafe void RestoreWeights(float* valueBuffer)
        {
            Contract.Requires(valueBuffer != null);

            for (int layerIndex = 0; layerIndex < buffers.Length; layerIndex++)
            {
                var layerBuffers = buffers[layerIndex];
                var layer = layers[layerIndex];

                Debug.Assert(layerBuffers.Length == layer.WeightedInputBuffers.Length);

                for (int bufferIndex = 0; bufferIndex < layerBuffers.Length; bufferIndex++)
                {
                    var buffRange = layerBuffers[bufferIndex];
                    var weightRange = layer.WeightedInputBuffers[bufferIndex].WeightBuffer;

                    Debug.Assert(buffRange.Size == weightRange.Size);

                    for (int buffIndex = buffRange.MinValue, weightIndex = weightRange.MinValue; buffIndex <= buffRange.MaxValue; buffIndex++, weightIndex++)
                    {
                        valueBuffer[weightIndex] = valueBuffer[buffIndex];
                    }
                }
            }
        }

        public unsafe void CopyFrom(float* valueBuffer, WeightRelatedValues tWeights)
        {
            Contract.Requires(valueBuffer != null);
            Contract.Requires(tWeights != null);

            for (int layerIndex = 0; layerIndex < buffers.Length; layerIndex++)
            {
                var layerBuffers = buffers[layerIndex];
                var otherLayerBuffers = tWeights.buffers[layerIndex];

                Debug.Assert(layerBuffers.Length == otherLayerBuffers.Length);

                for (int bufferIndex = 0; bufferIndex < layerBuffers.Length; bufferIndex++)
                {
                    var buffRange = layerBuffers[bufferIndex];
                    var otherBuffRange = otherLayerBuffers[bufferIndex];

                    Debug.Assert(buffRange.Size == otherBuffRange.Size);

                    for (int buffIndex = buffRange.MinValue, otherBuffIndex = otherBuffRange.MinValue; buffIndex <= buffRange.MaxValue; buffIndex++, otherBuffIndex++)
                    {
                        valueBuffer[buffIndex] = valueBuffer[otherBuffIndex];
                    }
                }
            }
        }
    }
}
