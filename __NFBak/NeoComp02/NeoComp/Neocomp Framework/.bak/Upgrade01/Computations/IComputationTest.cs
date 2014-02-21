using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IComputationTestContract<>))]
    public interface IComputationTest<T>
    {
        IComputationTestResult Test(IComputationalUnit<T> computationalUnit);
    }

    [ContractClassFor(typeof(IComputationTest<>))]
    class IComputationTestContract<T> : IComputationTest<T>
    {
        IComputationTestResult IComputationTest<T>.Test(IComputationalUnit<T> computationalUnit)
        {
            Contract.Requires(computationalUnit != null);
            Contract.Ensures(Contract.Result<IComputationTestResult>() != null);
            return null;
        }
    }
}
