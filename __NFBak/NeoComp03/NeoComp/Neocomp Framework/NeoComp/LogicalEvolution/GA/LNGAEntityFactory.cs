using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.GA;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Evolution;
using NeoComp.Networks.Computational.Logical;
using NeoComp.Networks;

namespace NeoComp.LogicalEvolution.GA
{
    public class LNGAEntityFactory : NaturalEntityFactory<LogicalNetworkGene>
    {
        #region Contruct

        public LNGAEntityFactory(TruthTable truthTableToSolve, LogicGateTypes gateTypeRestrictions = null)
        {
            Contract.Requires(truthTableToSolve != null);

            TruthTableToSolve = truthTableToSolve;
            GateTypeRestrictions = gateTypeRestrictions;

            if (gateTypeRestrictions != null)
            {
                allowedOperations = gateTypeRestrictions.Operations.ToArray();
            }
            else
            {
                allowedOperations = Definitions.AllOps;
            }
        }

        #endregion

        #region Props and Fields

        LogicalOperation[] allowedOperations;

        public TruthTable TruthTableToSolve { get; private set; }

        public LogicGateTypes GateTypeRestrictions { get; private set; }

        public GateTypeEvolutionMethod GateTypeEvolutionMethod { get; set; }

        int maxIndex = 100;

        public int MaxIndex
        {
            get { return maxIndex; }
            set
            {
                Contract.Requires(value >= 0);

                maxIndex = value;
            }
        }

        public bool CreateRecurrentNetworks { get; set; }

        double rateOfNodes = 0.25;

        public double RateOfNodes
        {
            get { return rateOfNodes; }
            set
            {
                Contract.Requires(value > 0.0 && value < 1.0);

                rateOfNodes = value;
            }
        }

        #endregion

        #region Init

        protected override void FillInitialRandomGeneSequence(LogicalNetworkGene[] sequence)
        {
            for (int idx = 0; idx < sequence.Length; idx++)
            {
                sequence[idx] = CreateRandomGene();
            }
        }

        #endregion

        #region Offspring

        protected override LogicalNetworkGene GetMutatedVersion(LogicalNetworkGene gene)
        {
            return CreateRandomGene();
        }

        #endregion

        #region LN Entity Create

        protected override Entity<DNA<LogicalNetworkGene>> CreateEntityInstance(DNA<LogicalNetworkGene> dna, IEnumerable<LogicalNetworkGene> dominantGeneSequence)
        {
            return LogicalNetworEntityFactory.Create(dna, dominantGeneSequence, GateTypeEvolutionMethod, TruthTableToSolve, GateTypeRestrictions);
        }

        #endregion

        #region Helpers

        protected virtual LogicalNetworkGene CreateRandomGene()
        {
            if (RandomGenerator.Random.NextDouble() <= rateOfNodes)
            {
                return CreateRandomNodeGene();
            }
            else
            {
                return CreateRandomConnGene();
            }
        }

        private LogicalConnectionGene CreateRandomConnGene()
        {
            bool isUpper = RandomGenerator.Random.Next(2) == 0;
            return new LogicalConnectionGene(CreateRandomConnIndex(isUpper), isUpper);
        }

        private LogicalNodeGene CreateRandomNodeGene()
        {
            return new LogicGateGene(CreateRandomNodeIndex(), CreateRandomOp());
        }

        private LogicalOperation CreateRandomOp()
        {
            if (GateTypeEvolutionMethod == GateTypeEvolutionMethod.Restrict)
            {
                return allowedOperations[RandomGenerator.Random.Next(allowedOperations.Length)];
            }
            else
            {
                return Definitions.AllOps[RandomGenerator.Random.Next(Definitions.AllOps.Length)];
            }
        }

        private int CreateRandomNodeIndex()
        {
            return RandomGenerator.Random.Next(maxIndex) + 1;
        }

        private int CreateRandomConnIndex(bool isUpper)
        {
            if (CreateRecurrentNetworks)
            {
                return RandomGenerator.Random.Next(-maxIndex, maxIndex + 1);
            }
            else if (isUpper)
            {
                return -(RandomGenerator.Random.Next(maxIndex) + 1);
            }
            else
            {
                return RandomGenerator.Random.Next(maxIndex) + 1;
            }
        }

        private bool IsValidConnIndex(ConnectionIndex index)
        {
            if (!CreateRecurrentNetworks && index.LowerNodeIndex <= index.UpperNodeIndex) return false;
            return true;
        }

        #endregion
    }
}
