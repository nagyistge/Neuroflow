using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    [ContractClass(typeof(IComputationalNetworkTestContract))]
    public interface IComputationalNetworkTest
    {
        IComputationTestResult Test(IComputationalNetwork network);
    }

    [ContractClassFor(typeof(IComputationalNetworkTest))]
    public class IComputationalNetworkTestContract : IComputationalNetworkTest
    {
        IComputationTestResult IComputationalNetworkTest.Test(IComputationalNetwork network)
        {
            Contract.Requires(network != null);
            Contract.Ensures(Contract.Result<IComputationTestResult>() != null);
            return null;
        }
    }
}
