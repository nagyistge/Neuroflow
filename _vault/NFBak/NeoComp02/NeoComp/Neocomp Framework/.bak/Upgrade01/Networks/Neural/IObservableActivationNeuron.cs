using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public interface IObservableActivationNeuron : IActivationNeuron
    {
        double Input { get; }
        
        double Output { get; }
    }
}
