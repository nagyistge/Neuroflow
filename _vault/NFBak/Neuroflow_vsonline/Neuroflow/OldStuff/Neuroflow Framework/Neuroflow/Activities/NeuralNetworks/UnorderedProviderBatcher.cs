using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.ComponentModel;
using Neuroflow.ComponentModel;
using Neuroflow.Core.Optimizations;
using Neuroflow.Core.Optimizations.BatchingStrategies;
using Neuroflow.Core;
using System.Activities.Presentation;
using Microsoft.VisualBasic.Activities;
using Neuroflow.Design.ActivityDesigners;
using System.Activities.Statements;

namespace Neuroflow.Activities.NeuralNetworks
{
    [Designer(typeof(UnorderedBatcherDesigner))]
    public sealed class UnorderedProviderBatcher : ProviderBatcher
    {
        #region Types

        class GetItemCount : CodeActivity<int>
        {
            [RequiredArgument]
            public InArgument<IUnorderedNeuralVectorsProvider> Provider { get; set; }

            protected override int Execute(CodeActivityContext context)
            {
                return Provider.Get(context).ItemCount;
            }
        }

        #endregion

        #region Properties

        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<IUnorderedNeuralVectorsProvider> Provider { get; set; }

        [Category(PropertyCategories.Algorithm)]
        [DisplayName("Last Result")]
        public InArgument<BatchExecutionResult> LastResult { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DisplayName("Reinitialization Freq.")]
        public InArgument<int> ReinitializationFrequency { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DisplayName("Use Cache")]
        public InArgument<bool> UseCache { get; set; }

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

            metadata.AddArgument(arg = new RuntimeArgument("Provider", typeof(IUnorderedNeuralVectorsProvider), ArgumentDirection.In, true));
            metadata.Bind(Provider, arg);

            metadata.AddArgument(arg = new RuntimeArgument("LastResult", typeof(BatchExecutionResult), ArgumentDirection.In));
            metadata.Bind(LastResult, arg);

            metadata.AddArgument(arg = new RuntimeArgument("ReinitializationFrequency", typeof(int), ArgumentDirection.In));
            metadata.Bind(ReinitializationFrequency, arg);

            metadata.AddArgument(arg = new RuntimeArgument("UseCache", typeof(bool), ArgumentDirection.In));
            metadata.Bind(UseCache, arg);
        }

        #endregion

        #region Impl

        protected override Activity<NeuralBatch> CreateImplementation()
        {
            return new UnorderedBatcher
            {
                BatchSize = new VisualBasicValue<int>("BatchSize"),
                UseCache = new VisualBasicValue<bool>("UseCache"),
                LastResult = new VisualBasicValue<BatchExecutionResult>("LastResult"),
                ReinitializationFrequency = new VisualBasicValue<int>("ReinitializationFrequency"),
                BatchingStrategy = BatchingStrategy,
                GetNextVectors = new ActivityFunc<IndexSet, NeuralVectors[]>
                {
                    Argument = new DelegateInArgument<IndexSet>("indexSet"),
                    Result = new DelegateOutArgument<NeuralVectors[]>("vectorsResult"),
                    Handler = new GetNextVectorsFromUnorderedProvider
                    {
                        IndexSet = new VisualBasicValue<IndexSet>("indexSet"),
                        Provider = new VisualBasicValue<IUnorderedNeuralVectorsProvider>("Provider"),
                        Result = new VisualBasicReference<NeuralVectors[]>("vectorsResult")
                    }
                },
                DoReinitializeVectorProvider = new ActivityAction
                {
                    Handler = new ReinitializeUnorderedProvider
                    {
                        Provider = new VisualBasicValue<IUnorderedNeuralVectorsProvider>("Provider")
                    }
                },
                ItemCountExpression = new GetItemCount
                {
                    Provider = new VisualBasicValue<IUnorderedNeuralVectorsProvider>("Provider") 
                }
            };
        }

        #endregion

        #region Tmpl

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new UnorderedProviderBatcher
            {
                DisplayName = "Unordered Provider Batcher",
                BatchSize = new InArgument<int>(200),
                UseCache = new InArgument<bool>(true)
            };
        } 

        #endregion
    }
}
