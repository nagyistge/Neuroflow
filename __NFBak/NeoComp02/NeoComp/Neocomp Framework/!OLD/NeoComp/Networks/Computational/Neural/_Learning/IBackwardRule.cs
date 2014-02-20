using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public interface IBackwardRule : ILearningRule
    {
        bool WantForwardIteration { get; }

        bool WantGradientInformation { get; }
    }
}
