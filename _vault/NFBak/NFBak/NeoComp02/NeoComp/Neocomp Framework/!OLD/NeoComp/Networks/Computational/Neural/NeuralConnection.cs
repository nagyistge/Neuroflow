using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "nConn")]
    public sealed class NeuralConnection : ComputationalConnection<double>, IBackwardConnection
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
