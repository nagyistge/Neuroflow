using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class GradientDescentLearningRule : SupervisedLearningRule
    {
        public GradientDescentLearningRule()
        {
            LearningRate = 0.01f;
            Momentum = 0.8f;
            Smoothing = false;
        }

        public float LearningRate { get; set; }

        public float Momentum { get; set; }

        public bool Smoothing { get; set; }

        public WeigthUpdateMode WeightUpdateMode { get; set; }

        protected internal override LearningAlgoOptimizationType OptimizationType
        {
            get { return LearningAlgoOptimizationType.GradientBased; }
        }

        protected internal override WeigthUpdateMode GetWeightUpdateMode()
        {
            return WeightUpdateMode;
        }

        protected override bool PropsEquals(LayerBehavior other)
        {
            var gd = other as GradientDescentLearningRule;
            return base.PropsEquals(other) && LearningRate == gd.LearningRate && Momentum == gd.Momentum && Smoothing == gd.Smoothing;
        }

        protected override int GenerateHashCode()
        {
            return base.GenerateHashCode() ^ LearningRate.GetHashCode() ^ Momentum.GetHashCode() ^ Smoothing.GetHashCode();
        }
    }
}
