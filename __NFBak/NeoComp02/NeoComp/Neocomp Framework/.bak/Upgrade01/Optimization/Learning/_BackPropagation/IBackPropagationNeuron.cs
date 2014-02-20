using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;

namespace NeoComp.Optimization.Learning
{
    public interface IBackPropagationNeuron : IObservableActivationNeuron, IDeltaBasedAdjustable
    {
        double Error { get; set; }

        new IDerivatableActivationFunction ActivationFunction { get; }
    }
}
