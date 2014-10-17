using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public abstract class NewNeuralConnectionFactory<T> : NewFactoryActivity<NeuralConnection>
        where T : NeuralConnection
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }

    public sealed class NewNeuralConnectionFactory : NewNeuralConnectionFactory<NeuralConnection> 
    {
    }
}
