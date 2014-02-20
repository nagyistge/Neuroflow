using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public sealed class UnorderedTrainingIteration : TrainingIteration
    {
        #region Properties

        protected override NNAlgorithm Algorithm
        {
            get { return NNAlgorithm.UnorderedTraining; }
        }

        #endregion

        #region Template

        protected override System.Activities.Activity CreateActivityTemplate()
        {
            return new UnorderedTrainingIteration
            {
                DisplayName = "Training Iteration",
                GetNextBatch = new ActivityFunc<NeuralVectorFlowBatch>
                {
                    Result = new DelegateOutArgument<NeuralVectorFlowBatch>("batchResult")
                }
            };
        }

        #endregion
    }
}
