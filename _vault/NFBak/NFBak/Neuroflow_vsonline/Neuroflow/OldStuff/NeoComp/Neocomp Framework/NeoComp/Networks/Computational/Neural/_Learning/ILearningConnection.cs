using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Neural
{
    public interface ILearningConnection : INeuralConnection
    {
        IEnumerable<ILearningRule> LearningRules { get; }
    }
}
