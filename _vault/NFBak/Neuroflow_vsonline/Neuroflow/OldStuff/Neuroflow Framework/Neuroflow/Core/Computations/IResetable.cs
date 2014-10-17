using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Computations
{
    [ContractClass(typeof(IResetableContract))]
    public interface IResetable
    {
        IReset GetReset();
    }

    [ContractClassFor(typeof(IResetable))]
    class IResetableContract : IResetable
    {
        #region IResetable Members

        IReset IResetable.GetReset()
        {
            Contract.Ensures(Contract.Result<IReset>() != null);
            return null;
        }

        #endregion
    }
}
