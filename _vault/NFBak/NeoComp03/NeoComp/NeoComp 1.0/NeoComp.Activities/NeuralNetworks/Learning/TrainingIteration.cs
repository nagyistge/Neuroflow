using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using NeoComp.Optimizations;
using NeoComp.NeuralNetworks.Learning;
using NeoComp.Activities.Internal;
using System.ComponentModel;
using NeoComp.Activities.Design;
using System.Activities.Presentation;
using NeoComp.Optimizations.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    [Designer(typeof(TrainingIterationDesigner))]
    public class TrainingIteration : NativeActivity<BatchExecutionResult>, IActivityTemplateFactory
    {
        [RequiredArgument]
        public InArgument<Training> Training { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Batch")]
        public ActivityFunc<NeuralBatch> GetNextBatch { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetNextBatch.IsNull()) metadata.AddValidationError("GetNextBatch function is expected.");
            
            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("Training", typeof(Training), ArgumentDirection.In));
            metadata.Bind(Training, arg);

            metadata.AddDelegate(GetNextBatch);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleFunc(GetNextBatch, OnGetNextBatchCompleted);
        }

        private void OnGetNextBatchCompleted(NativeActivityContext context, ActivityInstance instance, NeuralBatch batch)
        {
            if (batch != null)
            {
                var exec = GetExec(context);
                var result = exec.Execute(batch,
                    (r) => Console.WriteLine(r.AverageError));
                Result.Set(context, result);
            }
            else
            {
                Result.Set(context, BatchExecutionResult.Empty);
            }
        }

        internal virtual NeuralBatchExecution GetExec(NativeActivityContext context)
        {
            return Training.Get(context);
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate(target);
        }

        protected virtual TrainingIteration CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new TrainingIteration
            {
                DisplayName = "Training Iteration",
                GetNextBatch = new ActivityFunc<NeuralBatch> { Result = new DelegateOutArgument<NeuralBatch>("batchResult") }
            };
        }

        #endregion
    }
}
