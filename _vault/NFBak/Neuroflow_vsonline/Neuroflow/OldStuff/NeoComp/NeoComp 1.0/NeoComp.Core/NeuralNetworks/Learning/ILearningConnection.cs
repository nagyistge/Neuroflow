using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.NeuralNetworks.Learning
{
    public interface ILearningConnection : INeuralConnection
    {
        IEnumerable<ILearningRule> LearningRules { get; }
    }
}
