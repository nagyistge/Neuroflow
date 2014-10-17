using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.GA
{
    [ContractClass(typeof(IEntityContract))]
    public interface IEntity : IComparable
    {
        Guid UID { get; }

        object Plan { get; }
    }

    [ContractClassFor(typeof(IEntity))]
    class IEntityContract : IEntity
    {
        Guid IEntity.UID
        {
            get { return Guid.Empty; }
        }

        object IEntity.Plan
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return null;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
