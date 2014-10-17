using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    public class OptUnitPick : CompositeOptUnit
    {
        struct Entry
        {
            internal Entry(string optUnitID, object optUnitContext)
            {
                Contract.Requires(!String.IsNullOrEmpty(optUnitID));

                this.optUnitID = optUnitID;
                this.optUnitContext = optUnitContext;
            }
            
            string optUnitID;

            internal string OptUnitID
            {
                get { return optUnitID; }
            }

            object optUnitContext;

            internal object OptUnitContext
            {
                get { return optUnitContext; }
            }
        }

        public OptUnitPick(string id, int resolution, params OptUnit[] units)
            : this(id, resolution, (IEnumerable<OptUnit>)units)
        {
            Contract.Requires(units != null);
            Contract.Requires(units.Length > 0);
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
        }

        public OptUnitPick(string id, int resolution, IEnumerable<OptUnit> units)
            : base(id, units)
        {
            Contract.Requires(units != null);
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);

            Resolution = resolution;
        }

        public int Resolution { get; private set; }
        
        protected internal override object CreateContext()
        {
            return null;
        }

        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            if (context == null)
            {
                var randomUnit = Units[RandomGenerator.Random.Next(Units.Count)];
                return randomUnit.CreateEntityDataUnit(randomUnit.CreateContext());
            }
            else
            {
                var reg = (ItemRegistry<Entry>)context;
                var picked = reg.Pick(PickOtherMethod);
                return UnitDict[picked.OptUnitID].CreateEntityDataUnit(picked.OptUnitContext);
            }
        }

        private Entry PickOtherMethod(HashSet<Entry> except)
        {
            var except2 = new HashSet<string>(except.Select(e => e.OptUnitID));
            var randomUnit = Units[RandomGenerator.Random.Next(Units.Count)];
            while (except2.Contains(randomUnit.ID))
            {
                randomUnit = Units[RandomGenerator.Random.Next(Units.Count)];
            }
            return new Entry(randomUnit.ID, randomUnit.CreateContext());
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            var q = from du in eliteEntityDataUnits
                    group du by du.OptUnitID into g
                    let unit = UnitDict[g.Key]
                    let context = unit.ComputeNewContext(g)
                    select new Entry(unit.ID, context);

            var list = q.ToList();

            return new ItemRegistry<Entry>(Resolution, q);
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            return Units[RandomGenerator.Random.Next(Units.Count)].CreateRandomEntityDataUnit();
        }
    }
}
