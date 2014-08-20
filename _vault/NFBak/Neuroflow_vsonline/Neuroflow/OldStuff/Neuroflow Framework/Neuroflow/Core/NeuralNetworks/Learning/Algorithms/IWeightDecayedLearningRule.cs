using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public interface IWeightDecayedLearningRule : ILearningRule
    {
        WeightDecay WeightDecay { get; set; }
    }
}
