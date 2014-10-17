using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public sealed class GaussianHistoryAlgorithm : ErrorBasedLearningAlgorithm<GaussianHistoryRule>
    {
        #region Fields

        SortedList<double, WeightRelatedValues> history;

        bool fresh;

        bool initPhase;

        bool inOptPhase;

        int initIndex;

        #endregion

        #region Init

        protected override void Ininitalize(BufferAllocator allocator)
        {
            fresh = true;
            history = new SortedList<double, WeightRelatedValues>();
            for (int i = 0; i < Rule.Size; i++)
            {
                var values = new WeightRelatedValues(allocator, ConnectedLayers);
                history.Add(1000.0 + i, values);
            }
        }

        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            initPhase = true;
            inOptPhase = false;
            bool first = true;

            if (fresh)
            {
                foreach (var values in history.Values)
                {
                    if (first)
                    {
                        first = false;
                        values.SaveWeights(valueBuffer);
                    }
                    else
                    {
                        Randomize(valueBuffer, values);
                    }
                }
                fresh = false;
            }
            else
            {
                var old = history;
                history = new SortedList<double, WeightRelatedValues>();
                for (int i = 0; i < Rule.Size; i++)
                {
                    var values = old.Values[0];
                    if (first)
                    {
                        first = false;
                        values.SaveWeights(valueBuffer);
                    }
                    else
                    {
                        Randomize(valueBuffer, values);
                    }
                    history.Add(1000.0 + i, values);
                    old.RemoveAt(0);
                }
            }

            initIndex = 0;
            history.Values[initIndex].RestoreWeights(valueBuffer);
        }

        private unsafe void Randomize(float* valueBuffer, WeightRelatedValues values)
        {
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                {
                    var histValBuff = values.GetBuffer(layerIndex, buffIndex);
                    for (int weightIndex = 0; weightIndex < histValBuff.Size; weightIndex++)
                    {
                        float weight = GenerateRandomWeight();
                        valueBuffer[histValBuff.MinValue + weightIndex] = weight;
                    }
                }
            }
        }

        #endregion

        #region Run

        protected override unsafe void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            double normal = TransformError(averageError);
            if (initPhase)
            {
                initPhase = DoInitPhase(valueBuffer, normal);
                if (!initPhase)
                {
                    DoOptPhase(valueBuffer, normal);
                    inOptPhase = true;
                }
            }
            else
            {
                DoOptPhase(valueBuffer, normal);
            }
        }

        #region Init Phase

        private unsafe bool DoInitPhase(float* valueBuffer, double normal)
        {
            if (initIndex != -1 && !history.ContainsKey(normal))
            {
                var values = history.Values[initIndex];
                history.RemoveAt(initIndex);
                history.Add(normal, values);
            }
            
            int? index = null;
            for (int i = 0; i < history.Count; i++)
            {
                if (history.Keys[i] >= 1000.0)
                {
                    index = i;
                    break;
                }
            }

            if (index == null) return false;

            initIndex = index.Value;
            history.Values[initIndex].RestoreWeights(valueBuffer);
            return true;
        }

        #endregion

        #region Opt Phase

        private unsafe void DoOptPhase(float* valueBuffer, double normal)
        {
            if (inOptPhase) StoreResult(valueBuffer, normal);
            
            double mean = history.Keys[0];
            double stdDev = (history.Keys[history.Keys.Count - 1] - history.Keys[0]) * Rule.Scale;

            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                {
                    var weightBuff = layer.WeightedInputBuffers[buffIndex].WeightBuffer;
                    for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                    {
                        SetWeight(valueBuffer, layerIndex, buffIndex, weightIndex, weightBuff.MinValue, mean, stdDev);
                    }
                }
            }
        }

        private unsafe void SetWeight(float* valueBuffer, int layerIndex, int bufferIndex, int weightIndex, int weightBuffBeginIndex, double mean, double stdDev)
        {
            double pickedNormal = PickNormal(mean, stdDev);

            double prevNormal = GetNormalAt(history.Keys, 0);
            for (int prevI = 0, i = 1; i <= history.Count; i++, prevI++)
            {
                double currNormal = GetNormalAt(history.Keys, i);

                if (pickedNormal >= prevNormal && pickedNormal < currNormal)
                {
                    float d = (float)(pickedNormal - prevNormal); 
                    float prevWeight = GetWeightHistoryAt(valueBuffer, layerIndex, bufferIndex, weightIndex, prevI);
                    float currWeight = GetWeightHistoryAt(valueBuffer, layerIndex, bufferIndex, weightIndex, i);
                    float dW = currWeight - prevWeight;
                    float newWeight = prevWeight + (dW * d);

                    valueBuffer[weightBuffBeginIndex + weightIndex] = newWeight;

                    break;
                }

                prevNormal = currNormal;
            }
        }

        private unsafe float GetWeightHistoryAt(float* valueBuffer, int layerIndex, int bufferIndex, int weightIndex, int i)
        {
            if (i < 0 || i == history.Count) return GenerateRandomWeight();
            var buff = history.Values[i].GetBuffer(layerIndex, bufferIndex);
            return valueBuffer[buff.MinValue + weightIndex];
        }

        private double GetNormalAt(IList<double> normals, int i)
        {
            if (i < 0) return 0.0;
            if (i == normals.Count) return normals[normals.Count - 1] + (normals[normals.Count - 1] - normals[0]);
            return normals[i];
        }

        private static double PickNormal(double mean, double stdDev)
        {
            return Math.Abs(Statistics.GenerateGauss(0.0, stdDev)) + mean;
        }

        private unsafe void StoreResult(float* valueBuffer, double normal)
        {
            int lastIndex = history.Count - 1;
            if (!history.ContainsKey(normal) && history.Keys[lastIndex] >= normal)
            {
                var values = history.Values[lastIndex];
                history.RemoveAt(lastIndex);
                values.SaveWeights(valueBuffer);
                history.Add(normal, values);
            }
        }

        #endregion

        #region Common

        static double TransformError(double error)
        {
            var normal = 2.0 * Math.Sqrt(error);
            if (normal > 1.0) return 1.0;
            return normal;
        }

        unsafe private float GenerateRandomWeight()
        {
            //return (float)((Rule.Range * (float)RandomGenerator.Random.NextDouble()) * 2.0 - Rule.Range);
            return (float)Statistics.GenerateGauss(0.0, 0.3);
        }

        #endregion

        #endregion
    }
}
