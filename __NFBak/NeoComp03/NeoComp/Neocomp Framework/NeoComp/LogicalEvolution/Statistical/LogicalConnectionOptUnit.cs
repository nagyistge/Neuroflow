using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Evolution.Statistical;
using NeoComp.Core;

namespace NeoComp.LogicalEvolution.Statistical
{
    public sealed class LogicalConnectionOptUnit : LogicalOptUnit
    {
        private static IEnumerable<OptUnit> CreateOptUnit()
        {
            return new[] { new BooleanOptUnit("IsUpper") };
        }
        
        public LogicalConnectionOptUnit(string id, int resolution, IntRange indexRange)
            : base(id, resolution, indexRange, CreateOptUnit())
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
        }

        protected internal override LogicalNetworkGene CreateGene(EntityDataUnit unit)
        {
            var reg = GetRegistry(unit);
            int index = GetIndex(reg);
            bool isUpper = GetValue<bool>(reg, "IsUpper");
            return new LogicalConnectionGene(index, isUpper);
        }
    }
}
