using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public interface IWeightDecayedLearningRule : ILearningRule
    {
        WeightDecay WeightDecay { get; set; }
    }
}
