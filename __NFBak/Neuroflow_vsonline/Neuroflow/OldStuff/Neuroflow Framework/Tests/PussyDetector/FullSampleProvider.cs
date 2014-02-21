using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using Neuroflow.Core.Optimizations;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Drawing;

namespace PussyDetector
{
    public sealed class FullSampleProvider : UnorderedNeuralVectorsProvider
    {
        internal static readonly double?[] NullOutput = new double?[] { null };

        internal static readonly double?[] DetectedOutput = new double?[] { 1.0 };

        internal static readonly double?[] NotDetectedOutput = new double?[] { -1.0 };
        
        public FullSampleProvider(
            IEnumerable<FeatureStream> featureStreams, 
            int sampleSize, 
            int? subSampleSize = null,
            int numberOfComputationIterations = 1)
        {
            Contract.Requires(featureStreams != null);
            Contract.Requires(sampleSize > 0);
            Contract.Requires(subSampleSize == null || subSampleSize.Value < sampleSize);
            Contract.Requires(sampleSize % subSampleSize.Value == 0);
            Contract.Requires(numberOfComputationIterations > 0);

            SampleSize = sampleSize;
            SubSampleSize = subSampleSize == null ? SampleSize : subSampleSize.Value;
            this.featureStreams = featureStreams.ToList();
            NumberOfComputationIterations = numberOfComputationIterations;

            if (SubSampleSize != SampleSize)
            {
                int size = SampleSize / SubSampleSize;
                lookupArray = LookupArray.CreateWorm(size, WormLookupDirection.BorderToMiddle);
                //lookupArray = LookupArray.CreateSimple(size);
            }
        }

        public int NumberOfComputationIterations { get; private set; }

        public int SampleSize { get; private set; }

        public int SubSampleSize { get; private set; }

        Point[] lookupArray;

        List<FeatureStream> featureStreams;

        protected override UnorderedNeuralVectorsProvider.InitializationData Initialize()
        {
            return new InitializationData(featureStreams.Count, (SubSampleSize * SubSampleSize), 1);
        }

        protected override IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes)
        {
            foreach (var index in indexes)
            {
                var nv = CreateNeuralVectors(index);
                yield return nv;
            }
        }

        private NeuralVectors CreateNeuralVectors(int index)
        {
            var fs = featureStreams[index];
            var fa = fs.CreateFeatureArray(SampleSize);
            if (SubSampleSize == SampleSize)
            {
                var nv = new NeuralVectors(index, new VectorFlowEntry<double>(CreateInputVector(fa), CreateOutputVector(fs), NumberOfComputationIterations));
                return nv;
            }
            else
            {
                var entries = new LinkedList<VectorFlowEntry<double>>();
                int size = SampleSize / SubSampleSize;
                for (int lookupIndex = 0; lookupIndex < lookupArray.Length; lookupIndex++)
                {
                    Point lookupPoint = lookupArray[lookupIndex];
                    double?[] inputVector = new double?[SubSampleSize * SubSampleSize];
                    int inputIndex = 0;

                    for (int y = lookupPoint.Y; y < lookupPoint.Y + SubSampleSize; y++)
                    {
                        for (int x = lookupPoint.X; x < lookupPoint.X + SubSampleSize; x++)
                        {
                            inputVector[inputIndex++] = fa[y, x];
                        }
                    }

                    
                    //for (int y = 0; y < SampleSize; y += size)
                    //{
                    //    for (int x = 0; x < SampleSize; x += size)
                    //    {
                    //        int xp = x + lookupPoint.X;
                    //        int yp = y + lookupPoint.Y;
                    //        inputVector[inputIndex++] = fa[yp, xp];
                    //    }
                    //}

                    if (lookupIndex != lookupArray.Length - 1)
                    {
                        entries.AddLast(new VectorFlowEntry<double>(inputVector, NullOutput)); 
                    }
                    else
                    {
                        entries.AddLast(new VectorFlowEntry<double>(inputVector, CreateOutputVector(fs), NumberOfComputationIterations));
                    }
                }

                return new NeuralVectors(index, entries);
            }
        }

        private static double?[] CreateOutputVector(FeatureStream fs)
        {
            return fs.IsObjectDetected ? DetectedOutput : NotDetectedOutput;
        }

        private static double?[] CreateInputVector(double[,] fa)
        {
            return fa.Cast<double?>().ToArray();
        }

        internal IEnumerable<NeuralVectors> GetAllVectors()
        {
            int index = 0;

            foreach (var fs in featureStreams)
            {
                yield return CreateNeuralVectors(index++);
            }
        }
    }
}
