using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.ComputationalNetworks
{
    [ContractClass(typeof(IInterfacedContract))]
    public interface IInterfaced
    {
        int InputInterfaceLength { get; }

        int OutputInterfaceLength { get; }
    }

    [ContractClassFor(typeof(IInterfaced))]
    class IInterfacedContract : IInterfaced
    {
        int IInterfaced.InputInterfaceLength
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }
    }

}
