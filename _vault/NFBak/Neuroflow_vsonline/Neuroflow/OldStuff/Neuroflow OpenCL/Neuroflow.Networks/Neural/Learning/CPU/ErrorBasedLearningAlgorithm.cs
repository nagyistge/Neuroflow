using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning.CPU
{
    public abstract class ErrorBasedLearningAlgorithm<T> : LearningAlgorithm
         where T : ErrorBasedLearningRule
    {
        new public T Rule
        {
            get { return (T)base.Rule; }
        }

        #region Backward

        protected internal sealed override unsafe void BackwardIteration(float* valueBuffer, double averageError, bool isBatchIteration, int batchSize)
        {
            var mode = Rule.GetMode();
            if (isBatchIteration)
            {
                if (mode == LearningMode.Batch)
                {
                    BatchBackwardIteration(valueBuffer, batchSize, averageError);
                    var wd = Rule.WeightDecay;
                    if (wd != null && wd.IsEnabled) Decay(valueBuffer, wd);
                }
                else
                {
                    StochasticEndOfBatchBackwardIteration(valueBuffer, batchSize, averageError);
                    var wd = Rule.WeightDecay;
                    if (wd != null && wd.IsEnabled) Decay(valueBuffer, wd);
                }
            }
            else
            {
                StochasticBackwardIteration(valueBuffer, averageError);
            }
        }

        protected unsafe virtual void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
        }

        protected unsafe virtual void StochasticEndOfBatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
        }

        protected unsafe virtual void StochasticBackwardIteration(float* valueBuffer, double averageError)
        {
        }

        private unsafe void Decay(float* valueBuffer, WeightDecay wd)
        {
            foreach (var cl in ConnectedLayers)
            {
                DataParallel.Do(cl.WeightedInputBuffers.Length, RunParallel, ctx =>
                {
                    fixed (WeightedValueBuffer* buffs = cl.WeightedInputBuffers)
                    {
                        for (int buffIndex = ctx.WorkItemRange.MinValue; buffIndex <= ctx.WorkItemRange.MaxValue; buffIndex++) // TODO: Parallelize
                        {
                            var weightBuff = buffs[buffIndex].WeightBuffer;
                            for (int wi = weightBuff.MinValue; wi <= weightBuff.MaxValue; wi++)
                            {
                                valueBuffer[wi] = (float)wd.Decayed(valueBuffer[wi]);
                            }
                        }
                    }
                });
            }
        }

        #endregion
    }
}
