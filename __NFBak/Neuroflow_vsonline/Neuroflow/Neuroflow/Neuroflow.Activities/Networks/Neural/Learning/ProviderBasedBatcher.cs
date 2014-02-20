using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;
using System.Activities;
using System.Activities.Presentation;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public abstract class ProviderBasedBatcher : NativeActivity<NeuralVectorFlowBatch>, IActivityTemplateFactory
    {
        [RequiredArgument]
        public InArgument<int> BatchSize { get; set; }

        Activity<NeuralVectorFlowBatch> impl;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("BatchSize", typeof(int), ArgumentDirection.In, true));
            metadata.Bind(BatchSize, arg);

            impl = CreateImplementation();

            metadata.AddImplementationChild(impl);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleActivity(impl, OnImplExecureCompleted);
        }

        private void OnImplExecureCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectorFlowBatch result)
        {
            Result.Set(context, result);
        }

        protected abstract Activity<NeuralVectorFlowBatch> CreateImplementation();

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate(target);
        }

        protected abstract Activity CreateActivityTemplate(System.Windows.DependencyObject target);
    }
}
