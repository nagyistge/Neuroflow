using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    [ContractClass(typeof(IBackwardNodeContract))]
    public interface IBackwardNode
    {
        Backpropagator CreateBackprogatator();
    }

    [ContractClassFor(typeof(IBackwardNode))]
    class IBackwardNodeContract : IBackwardNode
    {
        Backpropagator IBackwardNode.CreateBackprogatator()
        {
            Contract.Ensures(Contract.Result<Backpropagator>() != null);
            Contract.Ensures(Contract.Result<Backpropagator>().Node == this);
            return null;
        }
    }
}
