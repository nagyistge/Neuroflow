using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Neuroflow.ComponentModel;
using Neuroflow.Core.Optimizations.NeuralNetworks;

namespace Neuroflow.Activities.NeuralNetworks
{
    public sealed class ReinitializeUnorderedProvider : AsyncCodeActivity
    {
        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<IUnorderedNeuralVectorsProvider> Provider { get; set; }

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
}
