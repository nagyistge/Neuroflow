using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks.ActivationFunctions;

namespace Neuroflow.Activities.ActivationFunctions
{
    public abstract class NewActivationFunction<T> : NewObjectActivity<IActivationFunction>
        where T : class, IActivationFunction
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
