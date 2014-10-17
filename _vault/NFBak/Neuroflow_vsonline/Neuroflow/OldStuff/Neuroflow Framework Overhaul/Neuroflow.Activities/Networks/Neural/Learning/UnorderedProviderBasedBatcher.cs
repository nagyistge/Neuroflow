using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Neuroflow.Activities.Design.ActivityDesigners;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Core.Vectors;
using Neuroflow.Core.Vectors.BatchingStrategies;
using Neuroflow.Core;
using System.Activities.Expressions;
using Microsoft.VisualBasic.Activities;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    [Designer(typeof(UnorderedBatcherDesigner))]
    public sealed class UnorderedProviderBasedBatcher : ProviderBasedBatcher
    {
        #region Types

        class GetItemCount : CodeActivity<int>
        {
            [RequiredArgument]
            public InArgument<IUnorderedNeuralVectorFlowProvider> Provider { get; set; }

            protected override int Execute(CodeActivityContext context)
            {
                return Provider.Get(context).ItemCount;
            }
        }

        sealed class GetNextVectorsFromUnorderedProvider : AsyncCodeActivity<NeuralVectorFlow[]>
        {
            [RequiredArgument]
            [Category(PropertyCategories.Algorithm)]
            public InArgument<IUnorderedNeuralVectorFlowProvider> Provider { get; set; }

            [RequiredArgument]
            [Category(PropertyCategories.Algorithm)]
            public InArgument<IndexSet> IndexSet { get; set; }

            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                var provider = Provider.Get(context);
                var set = IndexSet.Get(context);

                Func<IUnorderedNeuralVectorFlowProvider, IndexSet, NeuralVectorFlow[]> work = (p, s) =>
                {
                    return p.GetNext(s).ToArray();
                };

                context.UserState = work;

                return work.BeginInvoke(provider, set, callback, state);
            }

            protected override NeuralVectorFlow[] EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                return ((Func<IUnorderedNeuralVectorFlowProvider, IndexSet, NeuralVectorFlow[]>)context.UserState).EndInvoke(result);
            }
        }

        sealed class ReinitializeUnorderedProvider : AsyncCodeActivity
        {
            [RequiredArgument]
            [Category(PropertyCategories.Algorithm)]
            public InArgument<IUnorderedNeuralVectorFlowProvider> Provider { get; set; }

            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                var provider = Provider.Get(context);

                Action work = () =>
                {
                    provider.Reinitialize();
                };

                context.UserState = work;

                return work.BeginInvoke(callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                ((Action)context.UserState).EndInvoke(result);
            }
        }

        #endregion

        #region Properties

        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<IUnorderedNeuralVectorFlowProvider> Provider { get; set; }

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

        #endregion

        #region Meta

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (BatchingStrategy == null) metadata.AddValidationError("BatchingStrategy is required.");

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("Provider", typeof(IUnorderedNeuralVectorFlowProvider), ArgumentDirection.In, true));
            metadata.Bind(Provider, arg);

            metadata.AddArgument(arg = new RuntimeArgument("LastResult", typeof(BatchExecutionResult), ArgumentDirection.In));
            metadata.Bind(LastResult, arg);

            metadata.AddArgument(arg = new RuntimeArgument("ReinitializationFrequency", typeof(int), ArgumentDirection.In));
            metadata.Bind(ReinitializationFrequency, arg);
        }

        #endregion

        #region Impl

        protected override Activity<NeuralVectorFlowBatch> CreateImplementation()
        {
            return new UnorderedBatcher
            {
                BatchSize = new ArgumentValue<int>("BatchSize"),
                UseCache = UseCache,
                LastResult = new ArgumentValue<BatchExecutionResult>("LastResult"),
                ReinitializationFrequency = new ArgumentValue<int>("ReinitializationFrequency"),
                BatchingStrategy = BatchingStrategy,
                GetNextVectorFlow = new ActivityFunc<IndexSet, NeuralVectorFlow[]>
                {
                    Argument = new DelegateInArgument<IndexSet>("indexSet"),
                    Result = new DelegateOutArgument<NeuralVectorFlow[]>("vectorsResult"),
                    Handler = new GetNextVectorsFromUnorderedProvider
                    {
                        IndexSet = new VisualBasicValue<IndexSet>("indexSet"),
                        Provider = new VisualBasicValue<IUnorderedNeuralVectorFlowProvider>("Provider"),
                        Result = new VisualBasicReference<NeuralVectorFlow[]>("vectorsResult")
                    }
                },
                DoReinitializeVectorProvider = new ActivityAction
                {
                    Handler = new ReinitializeUnorderedProvider
                    {
                        Provider = new ArgumentValue<IUnorderedNeuralVectorFlowProvider>("Provider")
                    }
                },
                ItemCountExpression = new GetItemCount
                {
                    Provider = new ArgumentValue<IUnorderedNeuralVectorFlowProvider>("Provider")
                }
            };
        }

        #endregion

        #region Tmpl

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new UnorderedProviderBasedBatcher
            {
                DisplayName = "Unordered Provider Based Batcher",
                BatchSize = new InArgument<int>(200),
                UseCache = true
            };
        }

        #endregion
    }
}
