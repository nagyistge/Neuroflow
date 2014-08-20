using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public abstract class NewNeuralNodeFactory<T> : NewFactoryActivity<NeuralNode>
        where T : NeuralNode
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
