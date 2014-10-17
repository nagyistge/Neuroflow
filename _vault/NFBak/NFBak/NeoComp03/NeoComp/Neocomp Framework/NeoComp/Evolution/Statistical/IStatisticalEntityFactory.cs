using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    [ContractClass(typeof(IStatisticalEntityFactoryContract<>))]
    public partial interface IStatisticalEntityFactory<T>
        where T : class, IComparable<T>
    {
        IList<OptUnit> Units { get; }

        T CreateEntity(EntityDataUnit[] entityDataUnits);
    }

    [ContractClassFor(typeof(IStatisticalEntityFactory<>))]
    class IStatisticalEntityFactoryContract<T> : IStatisticalEntityFactory<T>
        where T : class, IComparable<T>
    {
        T IStatisticalEntityFactory<T>.CreateEntity(EntityDataUnit[] entityDataUnits)
        {
            IStatisticalEntityFactory<T> i = this;
            
            Contract.Requires(entityDataUnits != null);
            Contract.Requires(entityDataUnits.Length == i.Units.Count);
            Contract.Ensures(Contract.Result<T>() != null);
            return null;
        }

        IList<OptUnit> IStatisticalEntityFactory<T>.Units
        {
            get 
            {
                Contract.Ensures(Contract.Result<IList<OptUnit>>() != null);
                Contract.Ensures(Contract.Result<IList<OptUnit>>().Count > 0);
                return null;
            }
        }
    }
}
