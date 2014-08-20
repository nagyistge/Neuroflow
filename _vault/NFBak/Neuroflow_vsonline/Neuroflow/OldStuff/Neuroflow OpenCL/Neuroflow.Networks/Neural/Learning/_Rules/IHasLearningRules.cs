using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public interface IHasLearningRules
    {
        ReadOnlyCollection<LearningRule> LearningRules { get; }

        bool NeedsForwardIteration { get; }

        bool NeedsGradientInformation { get; }
    }
}
