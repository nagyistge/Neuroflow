using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public sealed class ValidationIteration : TrainingIteration
    {
        #region Properties

        protected override NNAlgorithm Algorithm
        {
            get { return NNAlgorithm.Validation; }
        }

        [Category(PropertyCategories.Algorithm)]
        public int EveryNthIteration { get; set; } // TODO: Support this property!

        #endregion

        #region Template

        protected override System.Activities.Activity CreateActivityTemplate()
        {
            return new ValidationIteration
            {
                DisplayName = "Validation Iteration",
                EveryNthIteration = 1,
                GetNextBatch = new ActivityFunc<NeuralVectorFlowBatch>
                {
                    Result = new DelegateOutArgument<NeuralVectorFlowBatch>("batchResult")
                }
            };
        }

        #endregion
    }
}
