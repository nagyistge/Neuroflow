using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using NeoComp.Activities.Design;
using NeoComp.Optimizations.BatchingStrategies;
using NeoComp.Activities.Internal;
using NeoComp.Optimizations;
using NeoComp.Optimizations.NeuralNetworks;
using Microsoft.VisualBasic.Activities;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    [Designer(typeof(UnorderedBatcherDesigner))]
    public sealed class UnorderedBatcher : Batcher
    {
        const string StrategyVarName = "Strategy";

        const string IterationsVarName = "Iterations";

        const string ItemCountVarName = "ItemCount";

        Variable<List<NeuralVectors>> cachedVectors 

        public Activity<int> ItemCount { get; set; }

        public InArgument<BatchExecutionResult> LastResult { get; set; }

        public InArgument<int> ReinitializationFrequency { get; set; }

        public InArgument<bool> UseCache { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Batching Strategy Factory")]
        public ActivityFunc<IFactory<BatchingStrategy>> GetBatchingStrategyFactory { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Vectors", Argument1Name = "Index Set")]
        public ActivityFunc<IndexSet, NeuralVectors[]> GetNextVectors { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Reinitialize Vector Provider")]
        public ActivityAction ReinitializeVectorProvider { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetBatchingStrategyFactory.IsNull()) metadata.AddValidationError("GetBatchingStrategyFactory function is expected.");
            if (GetNextVectors.IsNull()) metadata.AddValidationError("GetNextVectors function is expected.");
            if (ItemCount == null) metadata.AddValidationError("ItemCount expression expected.");

            metadata.AddDelegate(GetBatchingStrategyFactory);
            metadata.AddDelegate(GetNextVectors);
            metadata.AddDelegate(ReinitializeVectorProvider);

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("LastResult", typeof(BatchExecutionResult), ArgumentDirection.In));
            metadata.Bind(LastResult, arg);

            metadata.AddArgument(arg = new RuntimeArgument("ReinitializationFrequency", typeof(int), ArgumentDirection.In));
            metadata.Bind(ReinitializationFrequency, arg);

            metadata.AddChild(ItemCount);

            base.CacheMetadata(metadata);
        }

        #region Init

        protected override void Execute(NativeActivityContext context)
        {
            Begin(context);
        }

        private void Begin(NativeActivityContext context)
        {
            var vars = ComputationContext.GetVariables(context, this);

            if (!vars.Exists(IterationsVarName))
            {
                vars.Set(IterationsVarName, 0);
            }

            if (!vars.Exists(ItemCountVarName))
            {
                // ItemCount is not yet determined.
                ScheduleGetItemCount(context);
                return;
            }

            if (!vars.Exists(StrategyVarName))
            {
                context.ScheduleFunc(GetBatchingStrategyFactory, OnGetBatchingStrategyFactoryCompleted);
            }
            else
            {
                ScheduleGetNextVectors(context);
            }
        }

        private void OnGetBatchingStrategyFactoryCompleted(NativeActivityContext context, ActivityInstance instance, IFactory<BatchingStrategy> factory)
        {
            if (factory == null) throw new InvalidOperationException("Unordered Batcher '" + DisplayName + "' GetBatchingStrategyFactory method has returned a null value.");

            var strategy = factory.Create();
            var vars = ComputationContext.GetVariables(context, this);
            vars.Set(StrategyVarName, strategy);

            strategy.Initialize(vars.Get<int>(ItemCountVarName), GetBatchSize(context));

            ScheduleGetNextVectors(context);
        } 

        #endregion

        #region Get Next

        private void ScheduleGetNextVectors(NativeActivityContext context)
        {
            var vars = ComputationContext.GetVariables(context, this);

            var strategy = vars.Get<BatchingStrategy>(StrategyVarName);

            if (LastResult != null)
            {
                var obs = strategy as OptimizationBatchingStrategy;
                if (obs != null)
                {
                    var last = LastResult.Get(context);
                    if (!last.IsEmpty)
                    {
                        obs.SetLastResult(last);
                    }
                }
            }

            var indexSet = new IndexSet(strategy.GetNextIndexes());



            context.ScheduleFunc(GetNextVectors, indexSet, OnGetNextVectorsCompleted);
        }

        private void OnGetNextVectorsCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectors[] result)
        {
            if (ReinitializationFrequency != null && ReinitializeVectorProvider != null)
            {
                var freq = ReinitializationFrequency.Get(context);
                if (freq > 0)
                {
                    var vars = ComputationContext.GetVariables(context, this);
                    int iterations = vars.Get<int>(IterationsVarName);

                    if ((++iterations % freq) == 0)
                    {
                        context.ScheduleAction(ReinitializeVectorProvider, OnReinitializeVectorProviderCompleted);
                    }

                    vars.Set(IterationsVarName, iterations);
                }
            }

            GetNextVectorsDone(context, result, TrainingResetSchedule.None);
        }

        private void OnReinitializeVectorProviderCompleted(NativeActivityContext context, ActivityInstance instance)
        {
            // Delete ItemCount globale variable.
            // This will be determined on next vector call schedule.
            var vars = ComputationContext.GetVariables(context, this);
            vars.Delete(ItemCountVarName);
        }

        #endregion

        #region Get Item Count

        private void ScheduleGetItemCount(NativeActivityContext context)
        {
            context.ScheduleActivity(ItemCount, OnGetItemCountCompleted);
        }

        private void OnGetItemCountCompleted(NativeActivityContext context, ActivityInstance instance, int result)
        {
            // Item Count determinded, set to ItemCount global variable:
            var vars = ComputationContext.GetVariables(context, this);
            vars.Set(ItemCountVarName, result);

            // Continue from Begin:
            Begin(context);
        }

        #endregion

        protected override Batcher CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new UnorderedBatcher
            {
                DisplayName = "Unordered Batcher",
                BatchSize = new InArgument<int>(200),
                UseCache = new InArgument<bool>(true),
                GetBatchingStrategyFactory = new ActivityFunc<IFactory<BatchingStrategy>>
                {
                    Result = new DelegateOutArgument<IFactory<BatchingStrategy>>("strategyFactoryResult")
                },
                GetNextVectors = new ActivityFunc<IndexSet, NeuralVectors[]>
                {
                    Argument = new DelegateInArgument<IndexSet>("indexSet"),
                    Result = new DelegateOutArgument<NeuralVectors[]>("vectorsResult")
                },
                ReinitializeVectorProvider = new ActivityAction()
            };
        }
    }
}
