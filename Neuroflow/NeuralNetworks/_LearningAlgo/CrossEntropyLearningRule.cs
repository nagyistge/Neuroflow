using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public class CrossEntropyLearningRule : SupervisedLearningRule
    {
        public CrossEntropyLearningRule()
        {
            NarrowingRate = 0.95f;
            MutationChance = 0.05f;
            MeanMutationStrength = 0.05f;
            StdDevMutationStrength = 1.0f;
            PopulationSize = 10;
            WeightUpdateMode = WeigthUpdateMode.Offline;
        }

        public float NarrowingRate { get; set; }

        public float MutationChance { get; set; }

        public float MeanMutationStrength { get; set; }

        public float StdDevMutationStrength { get; set; }

        public int PopulationSize { get; set; }

        public WeigthUpdateMode WeightUpdateMode { get; set; }

        protected internal override LearningAlgoOptimizationType OptimizationType
        {
            get { return LearningAlgoOptimizationType.Global; }
        }

        protected internal override WeigthUpdateMode GetWeightUpdateMode()
        {
            return WeightUpdateMode;
        }
    }
}
