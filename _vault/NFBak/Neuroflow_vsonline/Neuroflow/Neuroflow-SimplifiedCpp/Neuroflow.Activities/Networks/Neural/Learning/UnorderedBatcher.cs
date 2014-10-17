using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Vectors;
using System.ComponentModel;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Core;
using Neuroflow.Core.Vectors.BatchingStrategies;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    [Designer(Designers.UnorderedBatcherDesigner)]
    public sealed class UnorderedBatcher : Batcher
    {
        const string StrategyVarName = "Strategy";

        const string IterationsVarName = "Iterations";

        const string ItemCountVarName = "ItemCount";

        const string CacheVarName = "Cache";

        const string CacheName = "UnorderedBatcherCache";

        Variable<LinkedList<NeuralVectorFlow>> cachedVectorFlows = new Variable<LinkedList<NeuralVectorFlow>>();

        Variable<bool> strategyHasJustInited = new Variable<bool>();

        [Category(PropertyCategories.Algorithm)]
        [DisplayName("Item Count Expression")]
        public Activity<int> ItemCountExpression { get; set; }

        [Category(PropertyCategories.Algorithm)]
        [DisplayName("Last Result")]
        public InArgument<BatchExecutionResult> LastResult { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DisplayName("Reinitialization Freq.")]
        public InArgument<int> ReinitializationFrequency { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DisplayName("Use Cache")]
        public bool UseCache { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DisplayName("Batching Strategy")]
        [RequiredArgument]
        public Activity<IFactory<BatchingStrategy>> BatchingStrategy { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Vector Flows", Argument1Name = "Index Set")]
        public ActivityFunc<IndexSet, NeuralVectorFlow[]> GetNextVectorFlow { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Do Reinitialize Vector Provider")]
        public ActivityAction DoReinitializeVectorProvider { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (BatchingStrategy == null) metadata.AddValidationError("BatchingStrategy is required.");
            if (GetNextVectorFlow.IsNull()) metadata.AddValidationError("GetNextVectors function is required.");
            if (ItemCountExpression == null) metadata.AddValidationError("ItemCountExpression is required.");

            metadata.AddChild(BatchingStrategy);
            metadata.AddDelegate(GetNextVectorFlow);
            metadata.AddDelegate(DoReinitializeVectorProvider);

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("LastResult", typeof(BatchExecutionResult), ArgumentDirection.In));
            metadata.Bind(LastResult, arg);

            metadata.AddArgument(arg = new RuntimeArgument("ReinitializationFrequency", typeof(int), ArgumentDirection.In));
            metadata.Bind(ReinitializationFrequency, arg);

            metadata.AddChild(ItemCountExpression);

            metadata.AddImplementationVariable(cachedVectorFlows);
            metadata.AddImplementationVariable(strategyHasJustInited);

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

            EnsureCache(context);

            if (!vars.Exists(StrategyVarName))
            {
                context.ScheduleActivity(BatchingStrategy, OnGetBatchingStrategyFactoryCompleted);
            }
            else
            {
                strategyHasJustInited.Set(context, false);
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
            strategyHasJustInited.Set(context, true);

            ScheduleGetNextVectors(context);
        }

        #endregion

        #region Get Next

        private void ScheduleGetNextVectors(NativeActivityContext context)
        {
            var vars = ComputationContext.GetVariables(context, this);

            var strategy = vars.Get<BatchingStrategy>(StrategyVarName);

            if (LastResult != null && !strategyHasJustInited.Get(context))
            {
                var obs = strategy as OptimizationBatchingStrategy;
                if (obs != null)
                {
                    var last = LastResult.Get(context);
                    if (!last.IsEmpty) obs.SetLastResult(last);
                }
            }

            var indexSet = new IndexSet(strategy.GetNextIndexes());

            if (UseCache)
            {
                // Get From Cache:
                var cache = vars.Get<SerializableCache>(CacheVarName).Cache;
                // Create variable:
                var vectorsFromCache = new LinkedList<NeuralVectorFlow>();
                cachedVectorFlows.Set(context, vectorsFromCache);

                var newIndexSet = new IndexSet(indexSet);

                foreach (var index in indexSet)
                {
                    string key = index.ToString();
                    var cached = cache[key] as NeuralVectorFlow;
                    if (cached != null)
                    {
                        // Cached, add to variable, and remove from indexes:
                        vectorsFromCache.AddLast(cached);
                        newIndexSet.Remove(index);
                    }
                }

                indexSet = newIndexSet;
            }

            if (indexSet.Count > 0)
            {
                // Are there any non-cached item requests? Get it!
                context.ScheduleFunc(GetNextVectorFlow, indexSet, OnGetNextVectorsCompleted);
            }
            else
            {
                // All items was in cache, proccess'em!
                ProcessNextVectors(context, null);
            }
        }

        private void OnGetNextVectorsCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectorFlow[] result)
        {
            ProcessNextVectors(context, result);
        }

        private void ProcessNextVectors(NativeActivityContext context, NeuralVectorFlow[] result)
        {
            bool reinitialize = false;
            IList<NeuralVectorFlow> finalResult = result;

            if (UseCache)
            {
                var vars = ComputationContext.GetVariables(context, this);
                var cache = vars.Get<SerializableCache>(CacheVarName).Cache;

                // Cache results:
                if (result != null)
                {
                    foreach (var vectors in result)
                    {
                        string key = vectors.Index.ToString();
                        cache[key] = vectors;
                    }
                }

                // Combine cached vectors with result:
                var vectorsFromCache = cachedVectorFlows.Get(context);
                cachedVectorFlows.Set(context, null);

                if (result != null)
                {
                    if (vectorsFromCache.Count != 0)
                    {
                        finalResult = result.Concat(vectorsFromCache).ToList();
                    }
                }
                else
                {
                    finalResult = vectorsFromCache.ToList();
                }
            }

            if (ReinitializationFrequency != null && DoReinitializeVectorProvider != null)
            {
                var freq = ReinitializationFrequency.Get(context);
                if (freq > 0)
                {
                    var vars = ComputationContext.GetVariables(context, this);
                    int iterations = vars.Get<int>(IterationsVarName);

                    if ((++iterations % freq) == 0)
                    {
                        context.ScheduleAction(DoReinitializeVectorProvider, OnReinitializeVectorProviderCompleted);
                        reinitialize = true;
                    }

                    vars.Set(IterationsVarName, iterations);
                }
            }

            var finalArray = finalResult as NeuralVectorFlow[];
            if (finalArray == null) finalArray = finalResult.ToArray();

            GetNextVectorsDone(context, finalArray, reinitialize ? ResetSchedule.AfterExecution : ResetSchedule.None);
        }

        private void OnReinitializeVectorProviderCompleted(NativeActivityContext context, ActivityInstance instance)
        {
            // Delete ItemCount globale variable.
            // This will be determined on next vector call schedule.
            var vars = ComputationContext.GetVariables(context, this);
            vars.Delete(ItemCountVarName);

            // Delete Batching Strategy (this will create new on next iteration)
            vars.Delete(StrategyVarName);

            // And delete cached stuff also:
            DeleteCache(context);

            Console.WriteLine("----- REINITIALIZED ------");
        }

        #endregion

        #region Get Item Count

        private void ScheduleGetItemCount(NativeActivityContext context)
        {
            context.ScheduleActivity(ItemCountExpression, OnGetItemCountCompleted);
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

        #region Caching

        private void EnsureCache(NativeActivityContext context)
        {
            if (UseCache)
            {
                var vars = ComputationContext.GetVariables(context, this);
                if (!vars.Exists(CacheVarName))
                {
                    var cache = new SerializableCache(CacheName);
                    vars.Set(CacheVarName, cache);
                }
            }
        }

        private void DeleteCache(NativeActivityContext context)
        {
            if (UseCache)
            {
                var vars = ComputationContext.GetVariables(context, this);
                SerializableCache cache;
                if (vars.TryGet(CacheVarName, out cache))
                {
                    cache.Dispose();
                    vars.Delete(CacheVarName);
                }
            }
        }

        #endregion

        protected override Batcher CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new UnorderedBatcher
            {
                DisplayName = "Unordered Batcher",
                BatchSize = new InArgument<int>(200),
                UseCache = true,
                GetNextVectorFlow = new ActivityFunc<IndexSet, NeuralVectorFlow[]>
                {
                    Argument = new DelegateInArgument<IndexSet>("indexSet"),
                    Result = new DelegateOutArgument<NeuralVectorFlow[]>("vectorsResult")
                },
                DoReinitializeVectorProvider = new ActivityAction()
            };
        }
    }
}
