using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.ComponentModel;
using Neuroflow.ComponentModel;
using Neuroflow.Core.Optimizations;

namespace Neuroflow.Activities.NeuralNetworks
{
    public sealed class ValidationIteration : TrainingIteration
    {
        #region Properties

        [Category(PropertyCategories.Algorithm)]
        public InArgument<int> EveryNthIteration { get; set; }

        #endregion

        #region Metadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            ExecuteTraining.DoValidationOnly = true;
        }

        #endregion

        #region Template

        protected override System.Activities.Activity CreateActivityTemplate()
        {
            return new ValidationIteration
            {
                DisplayName = "Validation Iteration",
                EveryNthIteration = new InArgument<int>(1),
                GetNextBatch = new ActivityFunc<NeuralBatch>
                {
                    Result = new DelegateOutArgument<NeuralBatch>("batchResult")
                }
            };
        }

        #endregion
    }
}
