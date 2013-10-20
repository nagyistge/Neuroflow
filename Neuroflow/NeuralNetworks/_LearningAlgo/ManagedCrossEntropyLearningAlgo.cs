using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    unsafe internal class ManagedCrossEntropyLearningAlgo : ManagedLearningAlgo<CrossEntropyLearningRule>
    {
        internal ManagedCrossEntropyLearningAlgo(CrossEntropyLearningRule rule, ReadOnlyCollection<TrainingNode> nodes) :
            base(rule, nodes)
        {
            weightCount = Nodes.SelectMany(n => n.Weights).Select(w => w.Size).Sum();
            populationWeights = new ManagedArray(Rule.PopulationSize * weightCount);
            fitnesses = new ManagedArray(Rule.PopulationSize);
            offspringWeights = new ManagedArray(weightCount);
        }

        public override LearningAlgoIterationType IterationTypes
        {
            get { return Rule.WeightUpdateMode == WeigthUpdateMode.Online ? LearningAlgoIterationType.SupervisedOnline : LearningAlgoIterationType.SupervisedOffline; }
        }

        int weightCount;

        ManagedArray populationWeights;

        ManagedArray fitnesses;

        ManagedArray offspringWeights;

        int fillPopulationIndex = 0;

        Action<int, ManagedArray> next;

        protected override void Initialize()
        {
            SetRandomWeights();
            fillPopulationIndex = 0;
            next = AddRandomWeightsToPopulation;
        }

        protected override void Run(int iterationCount, IDeviceArray error)
        {
            Debug.Assert(error != null);

            next(iterationCount, error.ToManaged());
        }

        private void SetRandomWeights()
        {
            for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
            {
                var inputWeights = Nodes[nodeIndex].Weights;
                for (int inputWeightIndex = 0; inputWeightIndex < inputWeights.Count; inputWeightIndex++)
                {
                    var weights = inputWeights[inputWeightIndex].ToManaged();
                    fixed (float* pWeights = weights.InternalArray)
                    {
                        var wPtr = weights.ToPtr(pWeights);
                        for (int widx = 0; widx < weights.Size; widx++) wPtr[widx] = RandomGenerator.NextFloat(-1, 1);
                    }
                }
            }
        }

        private void AddRandomWeightsToPopulation(int iterationCount, ManagedArray error)
        {
            WriteWeightsToPopulationMember(fillPopulationIndex++, GetErrorValue(iterationCount, error));
            if (fillPopulationIndex != Rule.PopulationSize)
            {
                SetRandomWeights();
            }
            else
            {
                SetOffspringWeights();
                next = AddOffspringWeightsToPopulation;
            }
        }

        private void SetOffspringWeights()
        {
            fixed (float* pPopulationWeights = populationWeights.InternalArray,
                pOffspringWeights = offspringWeights.InternalArray)
            {
                var populationWeightsPtr = populationWeights.ToPtr(pPopulationWeights);
                var offspringWeightsPtr = offspringWeights.ToPtr(pOffspringWeights);

                for (int widx = 0; widx < weightCount; widx++)
                {
                    double mean = 0.0;
                    double sum = 0.0;
                    double stdDev = 0.0;
                    double n = 0;
                    for (int popidx = 0; popidx < Rule.PopulationSize; popidx++)
                    {
                        float weight = populationWeightsPtr[popidx * weightCount + widx];
                        n++;
                        double delta = weight - mean;
                        mean += delta / n;
                        sum += delta * (weight - mean);
                    }
                    if (1 < n) stdDev = Math.Sqrt(sum / (n - 1));

                    stdDev = stdDev * Rule.NarrowingRate;      

                    Mutate(ref mean, ref stdDev);

                    offspringWeightsPtr[widx] = Math.Max(Math.Min((float)Statistics.GenerateGauss(mean, stdDev), 1), -1);
                }

                int owidx = 0;
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
                                wPtr[widx] = offspringWeightsPtr[owidx++];  
                            }
                        }
                    }
                }
            }
        }

        private void Mutate(ref double mean, ref double stdDev)
        {
            if (RandomGenerator.Random.NextDouble() <= Rule.MutationChance)
            {
                if (RandomGenerator.FiftyPercentChance)
                {
                    mean += RandomGenerator.NextDouble(-Rule.MeanMutationStrength, Rule.MeanMutationStrength);
                }
                else
                {
                    stdDev *= 1.0 + RandomGenerator.NextDouble(-Rule.StdDevMutationStrength, Rule.StdDevMutationStrength);
                }
            }
        }

        private void AddOffspringWeightsToPopulation(int iterationCount, ManagedArray error)
        {
            float fitness = GetErrorValue(iterationCount, error);
            fixed (float* pFitnesses = fitnesses.InternalArray)
            {
                var fitnessesPtr = fitnesses.ToPtr(pFitnesses);
                float maxFitness = float.MinValue;
                int maxIndex = 0;
                for (int popIndex = 0; popIndex < fitnesses.Size; popIndex++)
                {
                    if (fitnessesPtr[popIndex] > maxFitness)
                    {
                        maxFitness = fitnessesPtr[popIndex];
                        maxIndex = popIndex;
                    }
                }

                if (fitness < maxFitness) WriteOffspringToPopulationMember(maxIndex, fitness);
            }

            SetOffspringWeights();
        }

        private void WriteOffspringToPopulationMember(int popIndex, float fitness)
        {
            fitnesses.InternalArray[popIndex] = fitness;
            
            int popWeightIndex = popIndex * weightCount;
            fixed (float* pPopulationWeights = populationWeights.InternalArray,
                pOffspringWeights = offspringWeights.InternalArray)
            {
                var populationWeightsPtr = populationWeights.ToPtr(pPopulationWeights);
                var offspringWeightsPtr = offspringWeights.ToPtr(pOffspringWeights);

                for (int widx = 0; widx < weightCount; widx++)
                {
                    populationWeightsPtr[popWeightIndex++] = offspringWeightsPtr[widx];
                }
            }
        }

        private void WriteWeightsToPopulationMember(int popIndex, float fitness)
        {
            fitnesses.InternalArray[popIndex] = fitness;

            int popWeightIndex = popIndex * weightCount;
            fixed (float* pPopulationWeights = populationWeights.InternalArray)
            {
                var populationWeightsPtr = populationWeights.ToPtr(pPopulationWeights);
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
                                populationWeightsPtr[popWeightIndex++] = wPtr[widx];
                            }
                        }
                    }
                }
            }
        }

        private float GetErrorValue(int iterationCount, ManagedArray error)
        {
            return iterationCount == 0 ? error.ToManaged().InternalArray[0] : error.ToManaged().InternalArray[0] / (float)iterationCount;
        }
    }
}
