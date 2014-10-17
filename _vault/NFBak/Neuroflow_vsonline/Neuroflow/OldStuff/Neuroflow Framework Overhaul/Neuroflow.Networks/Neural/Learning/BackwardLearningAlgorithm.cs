using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class BackwardLearningAlgorithm<TRule> : LearningAlgorithm
        where TRule : BackwardLearningRule
    {
        #region Props

        new protected TRule Rule
        {
            get { return (TRule)base.Rule; }
        }

        public override bool WantForwardIteration
        {
            get { return false; }
        }

        #endregion

        #region Backward

        protected sealed override unsafe void BackwardIteration(bool isBatchIteration, double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            var mode = Rule.GetMode();
            if (isBatchIteration)
            {
                if (mode == LearningMode.Batch)
                {
                    BatchBackwardIteration(values, inputValueIndexes, outputValueIndexes);
                    var wd = Rule.WeightDecay;
                    if (wd != null) Decay(values, inputValueIndexes, wd);
                }
                else
                {
                    StochasticEndOfBatchBackwardIteration(values, inputValueIndexes, outputValueIndexes);
                    var wd = Rule.WeightDecay;
                    if (wd != null) Decay(values, inputValueIndexes, wd);
                }
            }
            else
            {
                StochasticBackwardIteration(values, inputValueIndexes, outputValueIndexes);
            }
        }

        protected unsafe virtual void BatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        }

        protected unsafe virtual void StochasticBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        }

        protected unsafe virtual void StochasticEndOfBatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        }

        private unsafe void Decay(double* values, InputValueIndexes* inputValueIndexes, WeightDecay wd)
        {
            int count = ValueCount;
            for (int i = 0; i < count; i++)
            {
                values[inputValueIndexes[i].WeightValueIndex] = wd.GetDecayed(values[inputValueIndexes[i].WeightValueIndex]);
            }
        }

        #endregion
    }
}
