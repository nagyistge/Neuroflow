using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimizations.BatchingStrategies;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public abstract class BatchingStrategyFactoryBlueprint<T> : Blueprint<IFactory<BatchingStrategy>>
        where T : BatchingStrategy
    {
        protected override IFactory<BatchingStrategy> CreateObject(System.Activities.NativeActivityContext context)
        {
            return CreateFactory();
        }

        protected abstract IFactory<T> CreateFactory();
    }
}
