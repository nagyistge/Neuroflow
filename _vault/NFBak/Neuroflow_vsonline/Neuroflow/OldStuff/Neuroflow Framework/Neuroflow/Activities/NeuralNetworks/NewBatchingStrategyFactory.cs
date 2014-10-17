using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Optimizations.BatchingStrategies;

namespace Neuroflow.Activities.NeuralNetworks
{
    public abstract class NewBatchingStrategyFactory<T> : NewFactoryActivity<BatchingStrategy>
        where T : BatchingStrategy
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
