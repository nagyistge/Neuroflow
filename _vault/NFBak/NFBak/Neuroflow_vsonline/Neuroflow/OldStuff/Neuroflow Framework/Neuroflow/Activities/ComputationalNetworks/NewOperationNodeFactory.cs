using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationalNetworks;

namespace Neuroflow.Activities.ComputationalNetworks
{
    public abstract class NewOperationNodeFactory<T> : NewFactoryActivity<OperationNode<T>>
        where T : struct
    {
    }
}
