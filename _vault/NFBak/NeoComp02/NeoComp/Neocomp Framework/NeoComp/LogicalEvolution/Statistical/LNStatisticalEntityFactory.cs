using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.Statistical;
using NeoComp.Networks.Computational.Logical;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.LogicalEvolution.Statistical
{
    public sealed class LNStatisticalEntityFactory : StatisticalEntityFactory<LogicalNetworEntity>
    {
        #region Contruct

        private static IEnumerable<LogicalOptUnitPick> GetOptUnits(
            int dnaSize,
            int maxIndex,
            int resolution,
            double rateOfNulls,
            double rateOfGates,
            GateTypeEvolutionMethod evolutionMethod,
            LogicGateTypes gateTypeRestrictions)
        {
            Contract.Requires(dnaSize > 0);
            Contract.Requires(maxIndex > 0);
            Contract.Requires(resolution > 1);
            Contract.Requires(rateOfNulls >= 0.0 && rateOfNulls < 1.0);
            Contract.Requires(rateOfGates > 0.0 && rateOfGates < 1.0);

            HashSet<LogicalOperation> allowedOperations;
            if (gateTypeRestrictions != null && evolutionMethod != GateTypeEvolutionMethod.Evolve)
            {
                allowedOperations = gateTypeRestrictions.Operations;
            }
            else
            {
                allowedOperations = new HashSet<LogicalOperation>(Definitions.AllOps);
            }

            var picks = new LinkedList<LogicalOptUnitPick>();
            for (int idx = 0; idx < dnaSize; idx++)
            {
                picks.AddLast(new LogicalOptUnitPick("Gene" + idx, resolution, IntRange.CreateInclusive(0, maxIndex), allowedOperations, rateOfNulls, rateOfGates)); 
            }
            return picks;
        }

        public LNStatisticalEntityFactory(
            TruthTable truthTableToSolve, 
            int dnaSize,
            int maxIndex,
            int resolution = 10,
            double rateOfNulls = 0.0,
            double rateOfGates = 0.25,
            GateTypeEvolutionMethod evolutionMethod = GateTypeEvolutionMethod.Restrict,
            LogicGateTypes gateTypeRestrictions = null)
            : base(GetOptUnits(dnaSize, maxIndex, resolution, rateOfNulls, rateOfGates, evolutionMethod, gateTypeRestrictions))
        {
            Contract.Requires(truthTableToSolve != null);
            Contract.Requires(dnaSize > 0);
            Contract.Requires(maxIndex > 0);
            Contract.Requires(resolution > 1);
            Contract.Requires(rateOfNulls >= 0.0 && rateOfNulls < 1.0);
            Contract.Requires(rateOfGates > 0.0 && rateOfGates < 1.0);

            TruthTableToSolve = truthTableToSolve;
            GateTypeRestrictions = gateTypeRestrictions;
            DNASize = dnaSize;
            MaxIndex = maxIndex;
            RateOfGates = rateOfGates;
            RateOfNulls = rateOfNulls;
            Resolution = resolution;
            EvolutionMethod = evolutionMethod;
        }

        #endregion

        #region Props and Fields

        public TruthTable TruthTableToSolve { get; private set; }

        public LogicGateTypes GateTypeRestrictions { get; private set; }

        public int Resolution { get; private set; }

        public int DNASize { get; private set; }

        public int MaxIndex { get; private set; }

        public double RateOfNulls { get; private set; }

        public double RateOfGates { get; private set; }

        public GateTypeEvolutionMethod EvolutionMethod { get; private set; }

        #endregion

        #region Create Entity

        public override LogicalNetworEntity CreateEntity(EntityDataUnit[] entityDataUnits)
        {
            var dna = new LinkedList<LogicalNetworkGene>();
            for (int idx = 0; idx < entityDataUnits.Length; idx++)
            {
                var pick = (LogicalOptUnitPick)Units[idx];
                var gene = pick.CreateGene(entityDataUnits[idx]);
                if (gene != null)
                {
                    dna.AddLast(gene);
                }
            }

            return LogicalNetworEntityFactory.Create(dna, EvolutionMethod, TruthTableToSolve, GateTypeRestrictions);
        } 

        #endregion
    }
}
