using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.GA;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Evolution;
using NeoComp.Networks.Computational.Logical;

namespace NeoComp.LogicalEvolution
{
    public sealed class LogicalNetworEntity : Entity<DNA<LogicalNetworkGene>>
    {
        internal LogicalNetworEntity(DNA<LogicalNetworkGene> dna, LogicalNetwork network, int errors, int numberOfNotAllowedGates)
            : base(dna)
        {
            Contract.Requires(dna != null);
            Contract.Requires(network != null);
            Contract.Requires(errors >= 0);
            Contract.Requires(numberOfNotAllowedGates >= 0);

            Network = network;
            Errors = errors;
            NumberOfNotAllowedGates = numberOfNotAllowedGates;
            IsPassed = Errors == 0 && NumberOfNotAllowedGates == 0;
        }

        public LogicalNetwork Network { get; private set; }

        public int Errors { get; private set; }

        public int NumberOfNotAllowedGates { get; private set; }

        public bool IsPassed { get; private set; }

        int rnd = RandomGenerator.Random.Next();

        protected override int GetComparingResult(Entity<DNA<LogicalNetworkGene>> other)
        {
            var lne = (LogicalNetworEntity)other;

            int c = Errors.CompareTo(lne.Errors);
            if (c != 0) return c;

            if (Errors == 0)
            {
                c = NumberOfNotAllowedGates.CompareTo(lne.NumberOfNotAllowedGates);
                if (c != 0) return c;

                if (NumberOfNotAllowedGates == 0)
                {
                    c = Network.NumberOfNodes.CompareTo(lne.Network.NumberOfNodes);
                    if (c != 0) return c;

                    c = Network.NumberOfConnections.CompareTo(lne.Network.NumberOfConnections);
                    if (c != 0) return c;

                    // If solved, try to decrase DNA length:
                    return Plan.Length.CompareTo(lne.Plan.Length);
                }
            }

            c = NumberOfNotAllowedGates.CompareTo(lne.NumberOfNotAllowedGates);
            if (c != 0) return c;

            c = -Network.NumberOfNodes.CompareTo(lne.Network.NumberOfNodes);
            if (c != 0) return c;

            c = -Network.NumberOfConnections.CompareTo(lne.Network.NumberOfConnections);
            if (c != 0) return c;

            c = rnd.CompareTo(lne.rnd);

            return c;
        }
    }
}
