using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.Vectors;
using System.Diagnostics;

namespace LeCun.Features
{
    public sealed class LeCunSampleProvider : UnorderedNeuralVectorFlowProvider
    {
        public LeCunSampleProvider(bool isTraining)
        {
            IsTraining = isTraining;
        }
        
        public bool IsTraining { get; private set; }

        int[] indexMap;
        
        protected override UnorderedNeuralVectorFlowProvider.InitializationData Initialize()
        {
            using (var ctx = new LeCunDataEntities())
            {
                int count = ctx.Samples.Count(s => s.IsTraining == IsTraining);
                indexMap = ctx.Samples.Where(s => s.IsTraining == IsTraining).OrderBy(s => s.ID).Select(s => s.ID).ToArray();
                return new InitializationData(count, LeCunDefinitions.RowCount * LeCunDefinitions.ColCount, LeCunDefinitions.ResultCount);
            }
        }

        protected override IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes)
        {
            using (var ctx = new LeCunDataEntities())
            {
                var selectIndexes = indexes.ToDictionary(i => indexMap[i]);
                foreach (var sample in (from s in ctx.Samples
                                        where s.IsTraining == IsTraining && selectIndexes.Keys.Contains(s.ID)
                                        select s))
                {
                    yield return ToFlow(selectIndexes[sample.ID], sample);
                }
            }
        }

        private NeuralVectorFlow ToFlow(int index, Sample sample)
        {
            return new NeuralVectorFlow(index, ToEntry(sample));
        }

        private VectorFlowEntry<double> ToEntry(Sample sample)
        {
            return new VectorFlowEntry<double>(CreateInputs(sample.ImageData), CreateOutputs(sample.Number));
        }

        private double?[] CreateInputs(byte[] imageData)
        {
            double?[] result = new double?[LeCunDefinitions.RowCount * LeCunDefinitions.ColCount];
            if (imageData.Length != result.Length) throw new InvalidOperationException("Invalid image data!");
            for (int idx = 0; idx < imageData.Length; idx++)
            {
                result[idx] = PixelToDouble(imageData[idx]);
            }
            return result;
        }

        private double?[] CreateOutputs(byte number)
        {
            double?[] result = new double?[LeCunDefinitions.ResultCount];
            for (int idx = 0; idx < LeCunDefinitions.ResultCount; idx++)
            {
                if (idx == number)
                {
                    result[idx] = 1.0;
                }
                else
                {
                    result[idx] = -1.0;
                }
            }
            return result;
        }

        static double PixelToDouble(byte pixel)
        {
            return ((double)pixel / 255.0) * 2.0 - 1.0;
        }

        internal IEnumerable<NeuralVectorFlow> GetAll()
        {
            using (var ctx = new LeCunDataEntities())
            {
                int index = 0;
                foreach (var sample in ctx.Samples.Where(s => s.IsTraining == IsTraining).OrderBy(s => s.ID))
                {
                    yield return ToFlow(index++, sample);
                }
            }
        }
    }
}
