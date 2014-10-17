using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Evolution.Statistical;
using NeoComp.Networks.Computational.Logical;

namespace NeoComp.LogicalEvolution.Statistical
{
    public sealed class LogicGateOptUnit : LogicalOptUnit
    {
        private static IEnumerable<OptUnit> CreateOptUnit(int resolution, ISet<LogicalOperation> operations)
        {
            Contract.Requires(resolution > 1);
            Contract.Requires(operations != null);
            Contract.Requires(operations.Count > 0); 

            return new[] { new SetOptUnit<LogicalOperation>("Operation", resolution, operations) }; 
        }

        public LogicGateOptUnit(string id, int resolution, IntRange indexRange, ISet<LogicalOperation> operations)
            : base(id, resolution, IntRange.CreateInclusive(1, indexRange.MaxValue), CreateOptUnit(resolution, operations))
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
            Contract.Requires(operations != null);
            Contract.Requires(operations.Count > 0);
        }

        protected internal override LogicalNetworkGene CreateGene(EntityDataUnit unit)
        {
            var reg = GetRegistry(unit);
            int index = GetIndex(reg);
            var operation = GetValue<LogicalOperation>(reg, "Operation");
            return new LogicGateGene(index, operation);
        }
    }
}
