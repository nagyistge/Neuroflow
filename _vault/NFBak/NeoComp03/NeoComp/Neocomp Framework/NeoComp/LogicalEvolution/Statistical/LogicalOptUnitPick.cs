using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.Statistical;
using NeoComp.Core;
using NeoComp.Networks.Computational.Logical;
using System.Diagnostics.Contracts;

namespace NeoComp.LogicalEvolution.Statistical
{
    public sealed class LogicalOptUnitPick : OptUnitPick
    {
        #region Const

        const string NullID = "Null";
        const string ConnectionID = "Connection";
        const string GateID = "Gate";

        #endregion

        #region Contruct

        private static IEnumerable<OptUnit> GetOptUnits(int resolution, IntRange indexRange, ISet<LogicalOperation> operations, bool nullAllowed)
        {
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
            Contract.Requires(operations != null);
            Contract.Requires(operations.Count > 0);

            var units = new LinkedList<OptUnit>();
            if (nullAllowed) units.AddLast(new NullOptUnit(NullID));
            units.AddLast(new LogicalConnectionOptUnit(ConnectionID, resolution, indexRange));
            units.AddLast(new LogicGateOptUnit(GateID, resolution, indexRange, operations));
            return units;
        }

        public LogicalOptUnitPick(string id, int resolution, IntRange indexRange, ISet<LogicalOperation> operations, double rateOfNulls, double rateOfGates)
            : base(id, resolution, GetOptUnits(resolution, indexRange, operations, rateOfNulls != 0.0))
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
            Contract.Requires(operations != null);
            Contract.Requires(operations.Count > 0);
            Contract.Requires(rateOfNulls >= 0.0 && rateOfNulls < 1.0);
            Contract.Requires(rateOfGates > 0.0 && rateOfGates < 1.0);

            RateOfGates = rateOfGates;
            RateOfNulls = RateOfNulls;
            connOptUnit = (LogicalConnectionOptUnit)UnitDict[ConnectionID];
            gateOptUnit = (LogicGateOptUnit)UnitDict[GateID];
        }

        #endregion

        #region Properties

        public double RateOfNulls { get; private set; }

        public double RateOfGates { get; private set; }

        LogicalConnectionOptUnit connOptUnit;

        LogicGateOptUnit gateOptUnit;

        #endregion

        #region Gene

        internal LogicalNetworkGene CreateGene(EntityDataUnit unit)
        {
            Contract.Requires(unit != null);

            switch (unit.OptUnitID)
            {
                case NullID:
                    return null;
                case ConnectionID:
                    return connOptUnit.CreateGene(unit);
                case GateID:
                    return gateOptUnit.CreateGene(unit);
                default:
                    throw new InvalidOperationException("Unknown OptUnit ID '" + unit.OptUnitID + "'.");
            }
        }

        #endregion
    }
}
