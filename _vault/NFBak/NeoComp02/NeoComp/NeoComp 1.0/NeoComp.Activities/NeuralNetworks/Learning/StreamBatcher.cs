using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NeoComp.Activities.Design;
using NeoComp.Optimizations;
using System.Activities;
using NeoComp.Activities.Internal;
using NeoComp.Optimizations.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public sealed class StreamBatcher : Batcher
    {
        Variable<List<NeuralVectors>> vectorList = new Variable<List<NeuralVectors>>();
        
        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Vectors")]
        public ActivityFunc<NeuralVectors> GetNextVectors { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetNextVectors.IsNull()) metadata.AddValidationError("GetNextVectors function is expected.");

            base.CacheMetadata(metadata);

            metadata.AddDelegate(GetNextVectors);
            metadata.AddImplementationVariable(vectorList);
        }

        protected override void Execute(NativeActivityContext context)
        {
            vectorList.Set(context, new List<VectorFlow<double>>(GetBatchSize(context)));

            ScheduleGetNextVectors(context);
        }

        private void ScheduleGetNextVectors(NativeActivityContext context)
        {
            context.ScheduleFunc(GetNextVectors, OnGetNextVectorsCompleted);
        }

        private void OnGetNextVectorsCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectors vectors)
        {
            var list = vectorList.Get(context);

            if (vectors != null) list.Add(vectors);

            if (vectors == null || list.Count == GetBatchSize(context))
            {
                GetNextVectorsDone(context, list.ToArray(), TrainingResetSchedule.None);
            }
            else if (vectors.IsEndOfStream)
            {
                GetNextVectorsDone(context, list.ToArray(), TrainingResetSchedule.AfterExecution);
            }
            else
            {
                ScheduleGetNextVectors(context);
            }

            list.Clear();
        }

        protected override Batcher CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new StreamBatcher
            {
                DisplayName = "Stream Batcher",
                BatchSize = new InArgument<int>(1),
                GetNextVectors = new ActivityFunc<NeuralVectors>
                {
                    Result = new DelegateOutArgument<NeuralVectors>("vectorsResult")
                }
            };
        }
    }
}
