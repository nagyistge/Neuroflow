using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.GA
{
    [ContractClass(typeof(IGAEntityFactoryContract<>))]
    public interface IGAEntityFactory<TPlan>
        where TPlan : class
    {
        Entity<TPlan>[] CreateInitialPopulation(int entityCount);

        Entity<TPlan> CreateOffspring(TPlan[] inheritedInformations);
    }

    [ContractClassFor(typeof(IGAEntityFactory<>))]
    class IGAEntityFactoryContract<T> : IGAEntityFactory<T>
        where T : class
    {
        Entity<T>[] IGAEntityFactory<T>.CreateInitialPopulation(int entityCount)
        {
            Contract.Requires(entityCount > 0);
            Contract.Ensures(Contract.Result<Entity<T>[]>() != null);
            Contract.Ensures(Contract.Result<Entity<T>[]>().Length == entityCount);
            return null;
        }

        Entity<T> IGAEntityFactory<T>.CreateOffspring(T[] inheritedInformations)
        {
            Contract.Requires(inheritedInformations != null);
            Contract.Requires(inheritedInformations.Length != 0);
            return null;     
        }
    }
}
