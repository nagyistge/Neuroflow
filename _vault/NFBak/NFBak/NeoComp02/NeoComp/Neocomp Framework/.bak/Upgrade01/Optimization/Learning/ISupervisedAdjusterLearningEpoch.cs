using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public interface ISupervisedAdjusterLearningEpoch
    {
        LearningMode LearningMode { get; }

        bool PreserveTestOrder { get; }

        int? MonteCarloSelect { get; }
    }
}
