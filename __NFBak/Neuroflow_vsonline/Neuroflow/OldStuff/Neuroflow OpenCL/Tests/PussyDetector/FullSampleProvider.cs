using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Drawing;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.Vectors;

namespace PussyDetector
{
    public sealed class FullSampleProvider : UnorderedNeuralVectorFlowProvider
    {
        internal static readonly float?[] NullOutput = new float?[] { null };

        internal static readonly float?[] DetectedOutput = new float?[] { 1.0f };

        internal static readonly float?[] NotDetectedOutput = new float?[] { -1.0f };
        
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

        public int MaxIterationLength
        {
            //get { return lookupArray.Length + (NumberOfComputationIterations - 1); }
            get { return SampleSize + (NumberOfComputationIterations - 1); }
        }

        Point[] lookupArray;

        List<FeatureStream> featureStreams;

        protected override UnorderedNeuralVectorFlowProvider.InitializationData Initialize()
        {
            //return new InitializationData(featureStreams.Count, SampleSize, 1);
            return new InitializationData(featureStreams.Count, SampleSize * SampleSize, 1);
        }

        protected override IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes)
        {
            foreach (var index in indexes)
            {
                var nv = CreateNeuralVectors(index);
                yield return nv;
            }
        }

        private NeuralVectorFlow CreateNeuralVectors(int index)
        {
            var fs = featureStreams[index];
            var fa = fs.CreateFeatureArray(SampleSize);
            if (SubSampleSize == SampleSize)
            {
                var nv = new NeuralVectorFlow(index, new VectorFlowEntry<float>(CreateInputVector(fa), CreateOutputVector(fs), NumberOfComputationIterations));
                return nv;
            }
            else
            {
                var entries = new LinkedList<VectorFlowEntry<float>>();
                for (int i = 0; i < SampleSize; i++)
                {
                    float[] inputVector = new float[SampleSize];
                    for (int j = 0; j < SampleSize; j++)
                    {
                        inputVector[j] = fa[i, j];
                    }
                    if (i != SampleSize - 1)
                    {
                        entries.AddLast(new VectorFlowEntry<float>(inputVector, NullOutput));
                    }
                    else
                    {
                        entries.AddLast(new VectorFlowEntry<float>(inputVector, CreateOutputVector(fs), NumberOfComputationIterations));
                    }
                }
                //int size = SampleSize / SubSampleSize;
                //for (int lookupIndex = 0; lookupIndex < lookupArray.Length; lookupIndex++)
                //{
                //    Point lookupPoint = lookupArray[lookupIndex];
                //    float[] inputVector = new float[SubSampleSize * SubSampleSize];
                //    int inputIndex = 0;

                //    for (int y = lookupPoint.Y; y < lookupPoint.Y + SubSampleSize; y++)
                //    {
                //        for (int x = lookupPoint.X; x < lookupPoint.X + SubSampleSize; x++)
                //        {
                //            inputVector[inputIndex++] = fa[y, x];
                //        }
                //    }

                //    if (lookupIndex != lookupArray.Length - 1)
                //    {
                //        entries.AddLast(new VectorFlowEntry<float>(inputVector, NullOutput)); 
                //    }
                //    else
                //    {
                //        entries.AddLast(new VectorFlowEntry<float>(inputVector, CreateOutputVector(fs), NumberOfComputationIterations));
                //    }
                //}

                return new NeuralVectorFlow(index, entries.ToArray());
            }
        }

        private static float?[] CreateOutputVector(FeatureStream fs)
        {
            return fs.IsObjectDetected ? DetectedOutput : NotDetectedOutput;
        }

        private static float[] CreateInputVector(float[,] fa)
        {
            return fa.Cast<float>().ToArray();
        }

        internal IEnumerable<NeuralVectorFlow> GetAllVectors()
        {
            int index = 0;

            foreach (var fs in featureStreams)
            {
                yield return CreateNeuralVectors(index++);
            }
        }
    }
}
