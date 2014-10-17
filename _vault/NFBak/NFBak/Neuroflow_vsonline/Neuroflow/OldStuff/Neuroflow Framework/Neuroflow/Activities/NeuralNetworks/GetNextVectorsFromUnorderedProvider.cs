using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Activities.NeuralNetworks
{
    public sealed class GetNextVectorsFromUnorderedProvider : AsyncCodeActivity<NeuralVectors[]>
    {
        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<IUnorderedNeuralVectorsProvider> Provider { get; set; }

        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<IndexSet> IndexSet { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var provider = Provider.Get(context);
            var set = IndexSet.Get(context);

            Func<IUnorderedNeuralVectorsProvider, IndexSet, NeuralVectors[]> work = (p, s) =>
            {
                return p.GetNextVectors(s).ToArray();
            };

            context.UserState = work;

            return work.BeginInvoke(provider, set, callback, state);
        }

        protected override NeuralVectors[] EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            return ((Func<IUnorderedNeuralVectorsProvider, IndexSet, NeuralVectors[]>)context.UserState).EndInvoke(result);
        }
    }
}
