﻿using System;
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
using System.Diagnostics;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    [Designer(typeof(UnorderedBatcherDesigner))]
    public sealed class UnorderedBatcher : Batcher
    {
        const string StrategyVarName = "Strategy";

        const string IterationsVarName = "Iterations";

        const string ItemCountVarName = "ItemCount";

        const string CacheVarName = "Cache";

        const string CacheName = "UnorderedBatcherCache";

        Variable<LinkedList<NeuralVectors>> cachedVectors = new Variable<LinkedList<NeuralVectors>>();

        Variable<bool> strategyHasJustInited = new Variable<bool>();

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

            metadata.AddArgument(arg = new RuntimeArgument("UseCache", typeof(bool), ArgumentDirection.In));
            metadata.Bind(UseCache, arg);

            metadata.AddChild(ItemCount);

            metadata.AddImplementationVariable(cachedVectors);
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
                context.ScheduleFunc(GetBatchingStrategyFactory, OnGetBatchingStrategyFactoryCompleted);
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
                    if (!last.IsEmpty)
                    {
                        obs.SetLastResult(last);
                    }
                }
            }

            var indexSet = new IndexSet(strategy.GetNextIndexes());

            if (IsCached(context))
            {
                // Get From Cache:
                var cache = vars.Get<SerializableCache>(CacheVarName).Cache;
                // Create variable:
                var vectorsFromCache = new LinkedList<NeuralVectors>();
                cachedVectors.Set(context, vectorsFromCache);

                var newIndexSet = new IndexSet(indexSet);

                foreach (var index in indexSet)
                {
                    string key = index.ToString();
                    var cached = cache[key] as NeuralVectors;
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
                context.ScheduleFunc(GetNextVectors, indexSet, OnGetNextVectorsCompleted);
            }
            else
            {
                // All items was in cache, proccess'em!
                ProcessNextVectors(context, null);
            }
        }

        private void OnGetNextVectorsCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectors[] result)
        {
            ProcessNextVectors(context, result);
        }

        private void ProcessNextVectors(NativeActivityContext context, NeuralVectors[] result)
        {
            bool reinitialize = false;
            IList<NeuralVectors> finalResult = result;

            if (IsCached(context))
            {
                var vars = ComputationContext.GetVariables(context, this); 
                var cache = vars.Get<SerializableCache>(CacheVarName).Cache;

                // Cache results:
                if (result != null)
                {
                    foreach (var vectors in result)
                    {
                        string key = vectors.ProviderIndex.ToString();
                        cache[key] = vectors;
                    }
                }

                // Combine cached vectors with result:
                var vectorsFromCache = cachedVectors.Get(context);
                cachedVectors.Set(context, null);

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
                        reinitialize = true;
                    }

                    vars.Set(IterationsVarName, iterations);
                }
            }

            GetNextVectorsDone(context, finalResult, reinitialize ? TrainingResetSchedule.AfterExecution : TrainingResetSchedule.None);
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

        #region Caching

        private bool IsCached(NativeActivityContext context)
        {
            return UseCache != null && UseCache.Get(context);
        }

        private void EnsureCache(NativeActivityContext context)
        {
            if (IsCached(context))
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
            if (IsCached(context))
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
