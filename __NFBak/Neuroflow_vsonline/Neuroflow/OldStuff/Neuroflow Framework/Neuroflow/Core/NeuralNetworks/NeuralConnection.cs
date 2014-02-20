using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.NeuralNetworks.Learning;

namespace Neuroflow.Core.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "nConn")]
    public sealed class NeuralConnection : ComputationConnection<double>, IBackwardConnection
    {
        [NonSerialized]
        BackwardValues backwardValues = new BackwardValues();

        BackwardValues IBackwardConnection.BackwardValues
        {
            get { return backwardValues; }
        }

        double INeuralConnection.Weight
        {
            get
            {
                return 1.0;
            }
            set
            {
                // Do Nothing.
            }
        }
    }
}
