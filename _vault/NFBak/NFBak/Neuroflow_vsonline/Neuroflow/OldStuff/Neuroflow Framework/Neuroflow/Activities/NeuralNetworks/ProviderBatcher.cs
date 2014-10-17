using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.Activities.Presentation;

namespace Neuroflow.Activities.NeuralNetworks
{
    public abstract class ProviderBatcher : NativeActivity<NeuralBatch>, IActivityTemplateFactory
    {
        [RequiredArgument]
        public InArgument<int> BatchSize { get; set; }

        Activity<NeuralBatch> impl;

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

        private void OnImplExecureCompleted(NativeActivityContext context, ActivityInstance instance, NeuralBatch result)
        {
            Result.Set(context, result);
        }

        protected abstract Activity<NeuralBatch> CreateImplementation();

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate(target);
        }

        protected abstract Activity CreateActivityTemplate(System.Windows.DependencyObject target);
    }
}
