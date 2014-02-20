using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Networks.Neural;
using System.Diagnostics.Contracts;
using System.Activities.Presentation;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public abstract class Batcher : NativeActivity<NeuralVectorFlowBatch>, IActivityTemplateFactory
    {
        [RequiredArgument]
        public InArgument<int> BatchSize { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("BatchSize", typeof(int), ArgumentDirection.In, true));
            metadata.Bind(BatchSize, arg);
        }

        protected int GetBatchSize(NativeActivityContext context)
        {
            Contract.Requires(context != null);

            int bs = BatchSize.Get(context);

            if (bs <= 0) throw new InvalidOperationException("Batcher BatchSize must be greater than zero.");

            return bs;
        }

        protected virtual void GetNextVectorsDone(NativeActivityContext context, NeuralVectorFlow[] vectorFlows, ResetSchedule resetSchedule)
        {
            if (vectorFlows.Length > 0)
            {
                var batch = new NeuralVectorFlowBatch(vectorFlows);
                Result.Set(context, batch);
            }
            else
            {
                Result.Set(context, null);
            }
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate(target);
        }

        protected abstract Batcher CreateActivityTemplate(System.Windows.DependencyObject target);

        #endregion
    }
}
