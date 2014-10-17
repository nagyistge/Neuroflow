using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Neuroflow.Core.Vectors;
using System.Activities.Presentation;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Networks.Neural.Learning;
using Neuroflow.Networks.Neural;
using Microsoft.VisualBasic.Activities;
using System.Activities.Expressions;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    [Designer(Designers.TrainingIterationDesigner)]
    public class TrainingIteration : NativeActivity<BatchExecutionResult>, IActivityTemplateFactory
    {
        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<Training> Training { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Batch")]
        public ActivityFunc<NeuralVectorFlowBatch> GetNextBatch { get; set; }

        internal ExecuteTraining ExecuteTraining { get; private set; }

        Variable<NeuralVectorFlowBatch> batchResult;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetNextBatch.IsNull()) metadata.AddValidationError("GetNextBatch function is required.");

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("Training", typeof(Training), ArgumentDirection.In, true));
            metadata.Bind(Training, arg);

            metadata.AddDelegate(GetNextBatch);

            batchResult = new Variable<NeuralVectorFlowBatch>();

            metadata.AddImplementationVariable(batchResult);

            ExecuteTraining =
                new ExecuteTraining
                {
                    Training = new ArgumentValue<Training>("Training"),
                    Batch = new InArgument<NeuralVectorFlowBatch>(batchResult),
                    Result = new ArgumentReference<BatchExecutionResult>("Result")
                };

            metadata.AddImplementationChild(ExecuteTraining);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleFunc(GetNextBatch, OnGetNextBatchCompleted);
        }

        private void OnGetNextBatchCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectorFlowBatch batch)
        {
            if (batch != null)
            {
                batchResult.Set(context, batch);
                context.ScheduleActivity(ExecuteTraining);
            }
            else
            {
                throw new InvalidOperationException("Training Iteration Get Next Batch Activity Method has returned a null value.");
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
                GetNextBatch = new ActivityFunc<NeuralVectorFlowBatch>
                {
                    Result = new DelegateOutArgument<NeuralVectorFlowBatch>("batchResult")
                }
            };
        }

        #endregion
    }
}
