using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using Neuroflow.Activities.ComputationalNetworks;

namespace Neuroflow.Activities.NeuralNetworks
{
    public sealed class NewActivationNeuron : NewOperationNodeFactory<double>
    {
        protected override Type ObjectType
        {
            get { return typeof(ActivationNeuron); }
        }
    }
}
