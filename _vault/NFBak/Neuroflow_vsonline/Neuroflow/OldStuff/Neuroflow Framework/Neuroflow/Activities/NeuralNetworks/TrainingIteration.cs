using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations;
using System.Activities.Presentation;
using System.ComponentModel;
using Neuroflow.Design.ActivityDesigners.Interface;
using Neuroflow.Core.NeuralNetworks.Learning;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using Neuroflow.ComponentModel;
using Neuroflow.Design.ActivityDesigners;
using Microsoft.VisualBasic.Activities;
using System.Activities.Statements;

namespace Neuroflow.Activities.NeuralNetworks
{
    [Designer(typeof(TrainingIterationDesigner))]
    public class TrainingIteration : NativeActivity<BatchExecutionResult>, IActivityTemplateFactory
    {
        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<Training> Training { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Batch")]
        public ActivityFunc<NeuralBatch> GetNextBatch { get; set; }

        internal ExecuteTraining ExecuteTraining { get; private set; }

        Variable<NeuralBatch> batchResult;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetNextBatch.IsNull()) metadata.AddValidationError("GetNextBatch function is required.");

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("Training", typeof(Training), ArgumentDirection.In, true));
            metadata.Bind(Training, arg);

            metadata.AddDelegate(GetNextBatch);

            batchResult = new Variable<NeuralBatch>();

            metadata.AddImplementationVariable(batchResult);

            ExecuteTraining =
                new ExecuteTraining
                {
                    Training = new InArgument<Training>(new VisualBasicValue<Training>("Training")),
                    Batch = new InArgument<NeuralBatch>(batchResult),
                    Result = new OutArgument<BatchExecutionResult>(new VisualBasicReference<BatchExecutionResult>("Result"))
                };

            metadata.AddImplementationChild(ExecuteTraining);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleFunc(GetNextBatch, OnGetNextBatchCompleted);
        }

        private void OnGetNextBatchCompleted(NativeActivityContext context, ActivityInstance instance, NeuralBatch batch)
        {
            if (batch != null)
            {
                batchResult.Set(context, batch);
                context.ScheduleActivity(ExecuteTraining);
            }
            else
            {
                Result.Set(context, null);
            }
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate();
        }

        protected virtual Activity CreateActivityTemplate()
        {
            return new TrainingIteration
            {
                DisplayName = "Training Iteration",
                GetNextBatch = new ActivityFunc<NeuralBatch>
                {
                    Result = new DelegateOutArgument<NeuralBatch>("batchResult")
                }
            };
        }

        #endregion
    }
}
