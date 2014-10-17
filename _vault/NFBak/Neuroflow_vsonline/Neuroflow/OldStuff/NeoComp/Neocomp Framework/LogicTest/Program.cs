using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Logical;
using NeoComp.LogicalEvolution.GA;
using NeoComp.Core;
using NeoComp.LogicalEvolution;
using NeoComp.LogicalEvolution.Statistical;
using NeoComp.Epoch;

namespace LogicTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Begin();
                Console.WriteLine("PAKTQ!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Begin()
        {
/*
0	0	0	0	0
0	1	0	0	1
1	0	0	0	1
1	1	0	1	0
0	0	1	0	1
0	1	1	1	0
1	0	1	1	0
1	1	1	1	1
*/

            var table = new TruthTable(
                3, 2,
                new[] 
                { 
                    new TruthTableEntry(new[] { false, false, false }, new[] { false, false }),
                    new TruthTableEntry(new[] { false, true, false }, new[] { false, true }),
                    new TruthTableEntry(new[] { true, false, false }, new[] { false, true }),
                    new TruthTableEntry(new[] { true, true, false }, new[] { true, false }),
                    new TruthTableEntry(new[] { false, false, true }, new[] { false, true }),
                    new TruthTableEntry(new[] { false, true, true }, new[] { true, false }),
                    new TruthTableEntry(new[] { true, false, true }, new[] { true, false }),
                    new TruthTableEntry(new[] { true, true, true }, new[] { true, true }),
                });

            var restrict = new LogicGateTypes(new[] { LogicGateType.NAND(2), LogicGateType.NOT() });

            var factory = new LNGAEntityFactory(table, restrict);

            factory.MaxIndex = 20;
            factory.ValidDNALenghtRange = IntRange.CreateInclusive(200, 250);
            factory.CrossoverChunkSize = IntRange.CreateExclusive(1, factory.ValidDNALenghtRange.MaxValue);
            factory.StoreParentSequences = false;
            factory.GateTypeEvolutionMethod = GateTypeEvolutionMethod.Restrict;

            factory.MutationParameters.MutationChunkSize = IntRange.CreateExclusive(1, factory.ValidDNALenghtRange.MaxValue);

            factory.MutationParameters.PointMutationChance = 0.01;

            factory.MutationParameters.DeletionMutationChance = 0.01;
            factory.MutationParameters.DuplicationMutationChance = 0.01;
            factory.MutationParameters.InsertionMutationChance = 0.01;
            factory.MutationParameters.TranslocationMutationChance = 0.01;
            factory.MutationParameters.InversionMutationChance = 0.01;

            var epoch = new LogicalGAEpoch(factory, 50, 25);

            epoch.NumberOfParentsRange = IntRange.CreateFixed(4);
            epoch.OffspringMovingChance = 0.1;
            epoch.BestSelectStdDev = 0.7;
            epoch.WorstSelectStdDev = 0.01;

            //var factory = new LNStatisticalEntityFactory(
            //    table,
            //    180,
            //    40,
            //    10,
            //    0.00,
            //    0.25,
            //    GateTypeEvolutionMethod.Evolve,
            //    restrict);

            //var epoch = new LogicalSAEpoch(factory, 50, 25, 5);

            bool done = false;

            LogicalNetworEntity best = null;

            while (!done)
            {
                epoch.Step();

                var cbest = (LogicalNetworEntity)epoch.BestEntity;

                if (cbest != null)
                {
                    if (best == null || cbest.CompareTo(best) < 0)
                    {
                        best = cbest;
                        Dump(epoch, best);
                    }
                    else if (epoch.CurrentIteration % 100 == 0)
                    {
                        Dump(epoch, best);
                    }
                }

                //if (Console.KeyAvailable)
                //{
                //    Console.ReadKey();
                //    done = true;
                //}
            }
        }

        private static void Dump(IEpoch epoch, LogicalNetworEntity best)
        {
            Console.WriteLine("{0} - Best error: {1}, NA.Gates: {2}, Nodes: {3}, Conns: {4}, DNA: {5}",
                epoch.CurrentIteration,
                best.Errors,
                best.NumberOfNotAllowedGates,
                best.Network.NumberOfNodes,
                best.Network.NumberOfConnections,
                best.Plan.Length);

            var gateEntries = best.Network.Entries.Where(e => e.NodeEntry.Node is LogicGate);
            foreach (var gateEntry in gateEntries)
            {
                Console.Write(new LogicGateType(((LogicGate)gateEntry.NodeEntry.Node).Operation, gateEntry.UpperConnectionEntries.Count));
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
