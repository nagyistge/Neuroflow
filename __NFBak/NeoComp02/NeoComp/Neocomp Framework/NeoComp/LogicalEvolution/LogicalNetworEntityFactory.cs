using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.GA;
using NeoComp.Networks.Computational.Logical;
using System.Diagnostics.Contracts;

namespace NeoComp.LogicalEvolution
{
    internal static class LogicalNetworEntityFactory
    {
        internal static LogicalNetworEntity Create(
            IEnumerable<LogicalNetworkGene> dominantGeneSequence,
            GateTypeEvolutionMethod evolutionMethod,
            TruthTable truthTableToSolve,
            LogicGateTypes gateTypeRestrictions)
        {
            Contract.Requires(dominantGeneSequence != null);
            Contract.Requires(truthTableToSolve != null);
            Contract.Requires(gateTypeRestrictions != null);

            var dna = new DNA<LogicalNetworkGene>(dominantGeneSequence.ToList());
            return Create(dna, dna.Genes[0], evolutionMethod, truthTableToSolve, gateTypeRestrictions);
        }
        
        internal static LogicalNetworEntity Create(
            DNA<LogicalNetworkGene> dna,
            IEnumerable<LogicalNetworkGene> dominantGeneSequence,
            GateTypeEvolutionMethod evolutionMethod,
            TruthTable truthTableToSolve,
            LogicGateTypes gateTypeRestrictions)
        {
            Contract.Requires(dna != null);
            Contract.Requires(dominantGeneSequence != null);
            Contract.Requires(truthTableToSolve != null);
            Contract.Requires(gateTypeRestrictions != null);
            
            LogicalNetworkFactory factory;
            if (evolutionMethod == GateTypeEvolutionMethod.Restrict)
            {
                factory = new LogicalNetworkFactory(truthTableToSolve.InputInterfaceLength, truthTableToSolve.OutputInterfaceLength, gateTypeRestrictions);
            }
            else // Evolve
            {
                factory = new LogicalNetworkFactory(truthTableToSolve.InputInterfaceLength, truthTableToSolve.OutputInterfaceLength);
            }

            var network = LogicalNetworkDNADecoder.CreateNetwork(factory, dominantGeneSequence);

            int numOfNAGates = 0;
            if (evolutionMethod == GateTypeEvolutionMethod.Evolve && gateTypeRestrictions != null)
            {
                foreach (var entry in network.EntryArray)
                {
                    var gate = entry.NodeEntry.Node as LogicGate;
                    if (gate != null)
                    {
                        var type = new LogicGateType(gate.Operation, entry.UpperConnectionEntryArray.Length);
                        if (!gateTypeRestrictions.Contains(type)) numOfNAGates++;
                    }
                }
            }

            int errors = new TruthTableComputation(network).ComputeError(truthTableToSolve);

            return new LogicalNetworEntity(dna, network, errors, numOfNAGates);
        }
    }
}
