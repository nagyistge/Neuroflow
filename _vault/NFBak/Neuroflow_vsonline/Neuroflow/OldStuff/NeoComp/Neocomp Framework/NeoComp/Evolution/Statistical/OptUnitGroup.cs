using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public class OptUnitGroup : CompositeOptUnit
    {
        protected OptUnitGroup(string id, params OptUnit[] units)
            : this(id, (IEnumerable<OptUnit>)units)
        {
            Contract.Requires(units != null);
            Contract.Requires(units.Length > 0);
            Contract.Requires(!string.IsNullOrEmpty(id));
        }

        protected OptUnitGroup(string id, IEnumerable<OptUnit> units)
            : base(id, units)
        {
            Contract.Requires(units != null);
            Contract.Requires(!string.IsNullOrEmpty(id));
        }

        protected internal override object CreateContext()
        {
            return Units.ToDictionary(u => u.ID, u => u.CreateContext());
        }

        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            var contexts = (Dictionary<string, object>)context;
            var dict = new Dictionary<string, EntityDataUnit>();
            for (int idx = 0; idx < contexts.Count; idx++)
            {
                var unit = Units[idx];
                var currentCtx = contexts[unit.ID];
                dict.Add(unit.ID, unit.CreateEntityDataUnit(currentCtx));
            }
            return new EntityDataUnit(ID, dict);
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            var dict = new Dictionary<string, EntityDataUnit>();
            for (int idx = 0; idx < Units.Count; idx++)
            {
                var unit = Units[idx];
                dict.Add(unit.ID, unit.CreateRandomEntityDataUnit());
            }
            return new EntityDataUnit(ID, dict);
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            var dicts = eliteEntityDataUnits.Select(du => (Dictionary<string, EntityDataUnit>)du.EntityData);
            var q = from dict in dicts
                    from kvp in dict
                    group kvp by kvp.Key into g
                    select new
                    {
                        ID = g.Key,
                        Context = UnitDict[g.Key].ComputeNewContext(g.Select(kvp => kvp.Value))
                    };
            return q.ToDictionary(item => item.ID, item => item.Context);
        }

        protected Dictionary<string, EntityDataUnit> GetRegistry(EntityDataUnit unit)
        {
            Contract.Requires(unit != null);

            var dict = unit.EntityData as Dictionary<string, EntityDataUnit>;
            if (unit.OptUnitID != ID || dict == null) throw new InvalidOperationException("Unit is not belongs to current group.");
            return dict;
        }

        protected T GetValue<T>(Dictionary<string, EntityDataUnit> registry, string name)
        {
            Contract.Requires(registry != null);
            Contract.Requires(!String.IsNullOrEmpty(name));

            EntityDataUnit dataUnit;
            try
            {
                if (registry.TryGetValue(name, out dataUnit))
                {
                    return (T)dataUnit.EntityData;
                }
            }
            catch (InvalidCastException)
            {
            }
            throw new InvalidOperationException("Index not found.");
        }
    }
}
