using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors.BatchingStrategies;

namespace Neuroflow.Activities.Vectors.BatchingStrategies
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
