using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationalNetworks;

namespace Neuroflow.Activities.ComputationalNetworks
{
    public abstract class NewComputationConnectionFactory<T> : NewFactoryActivity<ComputationConnection<T>>
        where T : struct
    {
    }
}
