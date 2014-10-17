using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    [ContractClass(typeof(IBackwardPropagatorContract))]
    public interface IBackwardPropagator
    {
        void BackPropagate(BackwardConnectionEntry[] outputs, BackwardConnectionEntry[] inputs, bool isNewBatch);
    }

    [ContractClassFor(typeof(IBackwardPropagator))]
    class IBackwardPropagatorContract : IBackwardPropagator
    {
        void IBackwardPropagator.BackPropagate(BackwardConnectionEntry[] outputs, BackwardConnectionEntry[] inputs, bool isNewBatch)
        {
            Contract.Requires(outputs != null);
            Contract.Requires(inputs != null);
        }
    }
}
