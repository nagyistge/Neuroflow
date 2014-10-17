using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public interface INeuron : INode
    {
    }
    
    public abstract class Neuron : ComputationalNode<Synapse, double>, INeuron
    {
    }
}
