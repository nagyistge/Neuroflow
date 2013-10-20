using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public abstract class SupervisedLearningRule : LearningBehavior
    {
        protected internal abstract LearningAlgoOptimizationType OptimizationType { get; }

        protected internal abstract WeigthUpdateMode GetWeightUpdateMode();

        protected override bool PropsEquals(LayerBehavior other)
        {
            return base.PropsEquals(other) && GetWeightUpdateMode() == ((SupervisedLearningRule)other).GetWeightUpdateMode();
        }

        protected override int GenerateHashCode()
        {
            return base.GenerateHashCode() ^ GetWeightUpdateMode().GetHashCode();
        }
    }
}
