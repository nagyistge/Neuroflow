using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimizations.BatchingStrategies;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public sealed class MonteCarloBatchingStrategyFactoryBlueprint : BatchingStrategyFactoryBlueprint<MonteCarloBatchingStrategy>
    {
        protected override IFactory<MonteCarloBatchingStrategy> CreateFactory()
        {
            return new Factory<MonteCarloBatchingStrategy>();
        }

        protected override System.Activities.Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new MonteCarloBatchingStrategyFactoryBlueprint { DisplayName = "Monte Carlo" };
        }
    }
}
