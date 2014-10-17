using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    public interface ILearningConnection : INeuralConnection
    {
        IEnumerable<ILearningRule> LearningRules { get; }
    }
}
