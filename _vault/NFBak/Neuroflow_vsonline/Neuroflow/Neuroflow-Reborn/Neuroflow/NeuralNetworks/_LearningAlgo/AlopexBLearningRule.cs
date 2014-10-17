using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public class AlopexBLearningRule : SupervisedLearningRule
    {
        public AlopexBLearningRule()
        {
            StepSizeB = 0.005f;
            StepSizeA = 0.005f;
            ForgettingRate = 0.35f;
            WeightUpdateMode = WeigthUpdateMode.Offline;
        }

        public float StepSizeA { get; set; }

        public float StepSizeB { get; set; }

        public float ForgettingRate { get; set; }

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
