using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    unsafe internal class ManagedAlopexBLearningAlgo : ManagedLearningAlgo<AlopexBLearningRule>
    {
        const int LastDeltaErrorBufferSize = 64;

        internal ManagedAlopexBLearningAlgo(AlopexBLearningRule rule, ReadOnlyCollection<TrainingNode> nodes) :
            base(rule, nodes)
        {
            int weightCount = Nodes.SelectMany(n => n.Weights).Select(w => w.Size).Sum();
            currentWeights = new ManagedArray(weightCount);
            lastWeights = new ManagedArray(weightCount);
            lastDeltaErrorBuffer = new ManagedArray(LastDeltaErrorBufferSize);
        }

        ManagedArray currentWeights, lastWeights;

        ManagedArray lastDeltaErrorBuffer;

        ManagedVectorUtils utils = new ManagedVectorUtils();

        ManagedDeviceArrayManagement man = new ManagedDeviceArrayManagement();

        int iterationNo;

        float lastError;

        public override LearningAlgoIterationType IterationTypes
        {
            get { return Rule.WeightUpdateMode == WeigthUpdateMode.Online ? LearningAlgoIterationType.SupervisedOnline : LearningAlgoIterationType.SupervisedOffline; }
        }

        protected override void Initialize()
        {
            utils.Zero(currentWeights);
            utils.Zero(lastDeltaErrorBuffer);
            utils.Zero(lastWeights);
            lastError = 0.0f;
        }

        protected override void Run(int iterationCount, IDeviceArray error)
        {
            float averageError = GetErrorValue(iterationCount, error.ToManaged());
            float deltaError = averageError - lastError;

            PushDeltaError(deltaError);

            if (iterationNo > 0) Step(averageError, deltaError);

            man.Copy(currentWeights, 0, lastWeights, 0, lastWeights.Size);
            SaveWeights();
            lastError = averageError;
            iterationNo++;
        }

        private void PushDeltaError(float deltaError)
        {
            fixed (float* p = lastDeltaErrorBuffer.InternalArray)
            {
                var ptr = lastDeltaErrorBuffer.ToPtr(p);
                for (int i = Math.Min(LastDeltaErrorBufferSize - 1, iterationNo); i > 0; i--)
                {
                    ptr[i] = ptr[i - 1];
                }
                ptr[0] = deltaError;
            }
        }

        private void Step(float averageError, float deltaError)
        {
            float cDiv = CalculateCDiv();

            fixed (float* pLastWeights = lastWeights.InternalArray)
            {
                var lastWeightsPtr = lastWeights.ToPtr(pLastWeights);
                int lwidx = 0;
                for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
                {
                    var inputWeights = Nodes[nodeIndex].Weights;
                    for (int inputWeightIndex = 0; inputWeightIndex < inputWeights.Count; inputWeightIndex++)
                    {
                        var weights = inputWeights[inputWeightIndex].ToManaged();
                        fixed (float* pWeights = weights.InternalArray)
                        {
                            var wPtr = weights.ToPtr(pWeights);
                            for (int widx = 0; widx < weights.Size; widx++)
                            {
                                float deltaWeight = wPtr[widx] - lastWeightsPtr[lwidx];

                                float c = cDiv != 0.0f ? ((float)Math.Sign(deltaWeight) * deltaError) / cDiv : 0.0f;
                                float p = LogisticFunction(c);
                                float ksi = Math.Sign(RandomGenerator.Random.NextDouble() - p);

                                lastWeightsPtr[lwidx++] = wPtr[widx];

                                float w = (float)(wPtr[widx] - (Rule.StepSizeB * deltaWeight * deltaError) + (Rule.StepSizeA * ksi));

                                wPtr[widx] = Math.Max(Math.Min(w, 1), -1);
                            }
                        }
                    }
                }
            }
        }

        private float CalculateCDiv()
        {
            float cDiv = 0.0f;
            fixed (float* p = lastDeltaErrorBuffer.InternalArray)
            {
                var ptr = lastDeltaErrorBuffer.ToPtr(p);

                for (int i = iterationNo, n = 0; i >= 1 && n + 1 < LastDeltaErrorBufferSize; i--, n++)
                {
                    float de = ptr[n + 1];
                    float l = Rule.ForgettingRate * (float)Math.Pow(Rule.ForgettingRate - 1.0f, n);
                    cDiv += l * Math.Abs(de);
                }
            }
            return cDiv;
        }

        private void SaveWeights()
        {
            fixed (float* pLastWeights = currentWeights.InternalArray)
            {
                var lastWeightsPtr = currentWeights.ToPtr(pLastWeights);
                int lwidx = 0;
                for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
                {
                    var inputWeights = Nodes[nodeIndex].Weights;
                    for (int inputWeightIndex = 0; inputWeightIndex < inputWeights.Count; inputWeightIndex++)
                    {
                        var weights = inputWeights[inputWeightIndex].ToManaged();
                        fixed (float* pWeights = weights.InternalArray)
                        {
                            var wPtr = weights.ToPtr(pWeights);
                            for (int widx = 0; widx < weights.Size; widx++)
                            {
                                lastWeightsPtr[lwidx++] = wPtr[widx];
                            }
                        }
                    }
                }
            }
        }

        private float LogisticFunction(float val)
        {
            return (1.0f / (1.0f + (float)Math.Exp(-val)));
        }

        private float GetErrorValue(int iterationCount, ManagedArray error)
        {
            float v = iterationCount == 0 ? error.ToManaged().InternalArray[0] : error.ToManaged().InternalArray[0] / (float)iterationCount;

            return (v + 1.0f) / 2.0f;
        }
    }
}
