using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Logical;
using NeoComp.Core;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Optimization.GeneticNetworks;
using System.Diagnostics;

namespace TEST.Logic
{
    public static class LogicGATest
    {
        /*
        const int groupCount = 25;

        const int groupSize = 25;
        var parameters = new ConfiguredLogicalNetworkParameters
            {
                InputInterfaceLength = test.InputSetSize,
                OutputInterfaceLength = test.OutputSetSize,
                MaxConnectionIndex = 40,                             
                ConnectionCountRange = new InclusiveRange(100, 150),
                FeedForward = true,         
                IndexMutationChance = 0.5,  
                AvailableGates = new HashSet<LogicGateType>
                {
                    LogicGateType.AND,
                    LogicGateType.NAND,
                    LogicGateType.NOR,
                    LogicGateType.OR,
                    LogicGateType.XNOR,
                    LogicGateType.XOR
                },
                Test = test
            };
          
         factory.MutationParameters.MutationChunkSize = new InclusiveRange(parameters.ConnectionCountRange.Max);
            factory.MutationParameters.CrossoverChunkSize = new InclusiveRange(parameters.ConnectionCountRange.Max);

            factory.MutationParameters.PointMutationChance = 0.04;

            factory.MutationParameters.DeletionMutationChance = 0.001;
            factory.MutationParameters.DuplicationMutationChance = 0.001;
            factory.MutationParameters.InsertionMutationChance = 0.001;
            factory.MutationParameters.TranslocationMutationChance = 0.001;
            factory.MutationParameters.InversionMutationChance = 0.001;
          
          population.ChanceOfMigration = 0.1;

         */

        const string problem =
            //@"
            //0  0  0  0 = 1  1  1  1  1  1  0  
            //0  0  0  1 = 0  1  1  0  0  0  0  
            //0  0  1  0 = 1  1  0  1  1  0  1  
            //0  0  1  1 = 1  1  1  1  0  0  1  
            //0  1  0  0 = 0  1  1  0  0  1  1  
            //0  1  0  1 = 1  0  1  1  0  1  1  
            //0  1  1  0 = 1  0  1  1  1  1  1  
            //0  1  1  1 = 1  1  1  0  0  0  0  
            //1  0  0  0 = 1  1  1  1  1  1  1  
            //1  0  0  1 = 1  1  1  1  0  1  1 
            //";

@"
0 0 0 0 = 0 0 0 0 0 0 1
0 0 0 1 = 1 0 0 1 1 1 1
0 0 1 0 = 0 0 1 0 0 1 0
0 0 1 1 = 0 0 0 0 1 1 0
0 1 0 0 = 1 0 0 1 1 0 0
0 1 0 1 = 0 1 0 0 1 0 0
0 1 1 0 = 0 1 0 0 0 0 0
0 1 1 1 = 0 0 0 1 1 1 1
1 0 0 0 = 0 0 0 0 0 0 0
1 0 0 1 = 0 0 0 0 1 0 0
1 0 1 0 = 0 0 0 1 0 0 0
1 0 1 1 = 1 1 0 0 0 0 0
1 1 0 0 = 0 1 1 0 0 0 1
1 1 0 1 = 1 0 0 0 0 1 0
1 1 1 0 = 0 1 1 0 0 0 0
1 1 1 1 = 0 1 1 1 0 0 0
";

        static Stopwatch sw = new Stopwatch();

        static LogicalNetworkTest test = LogicalNetworkTest.Create(problem);

        const int groupCount = 25;

        const int groupSize = 25;

        public static void Begin()
        {
            var parameters = new ConfiguredLogicalNetworkParameters
            {
                InputInterfaceLength = test.InputSize,
                OutputInterfaceLength = test.OutputSize,
                //MaxConnectionIndex = 40,
                //ConnectionCountRange = new InclusiveRange(100, 200),
                MaxConnectionIndex = 60,
                ConnectionCountRange = IntRange.CreateFixed(600),
                FeedForward = true,         
                IndexMutationChance = 0.5,  
                AvailableGates = new HashSet<LogicGateType>
                {
                    LogicGateType.AND,
                    LogicGateType.NAND,
                    LogicGateType.NOR,
                    LogicGateType.OR,
                    LogicGateType.XNOR,
                    LogicGateType.XOR
                },
                Test = test
            };

            var factory = new ConfiguredLogicalNetworkBodyFactory(parameters);

            factory.MutationParameters.MutationChunkSize = IntRange.CreateExclusive(1, parameters.ConnectionCountRange.MaxValue / 2);
            factory.MutationParameters.CrossoverChunkSize = IntRange.CreateExclusive(1, parameters.ConnectionCountRange.MaxValue);
            //factory.MutationParameters.MutationChunkSize = new InclusiveRange(100);
            //factory.MutationParameters.CrossoverChunkSize = new InclusiveRange(10);

            factory.MutationParameters.PointMutationChance = 0.01;

            factory.MutationParameters.DeletionMutationChance = 0.001;
            factory.MutationParameters.DuplicationMutationChance = 0.001;
            factory.MutationParameters.InsertionMutationChance = 0.001;
            factory.MutationParameters.TranslocationMutationChance = 0.001;
            factory.MutationParameters.InversionMutationChance = 0.001;

            var population = new ConfiguredLogicalNetworkPopulation();
            population.ChanceOfMigration = 0.05;

            for (int idx = 0; idx < groupCount; idx++)
            {
                var group = new ConfiguredLogicalNetworkGroup(
                    factory, 
                    groupSize);
                //group.SelectionStrategy = new RandomSelectionStrategy();
                group.ParentCount = IntRange.CreateFixed(3);
                population.Groups.Add(group);
            }

            var context = new GAContext(population);
            context.BestBodyArrived += OnBestArrived;

            sw.Start();
            context.Start();
            Console.ReadKey();
            context.Stop();
            sw.Stop();
        }

        static readonly object cl = new object();

        static void OnBestArrived(object sender, BestBodyArrivedToPopulationEventArgs e)
        {
            lock (cl)
            {
                var plannedNetwork = (ConfiguredLogicalNetworkBody)e.Body;
                var network = plannedNetwork.CreateNetwork();
                string state = plannedNetwork.IsFunctional ? "*** PASSED ***" : "--- BEST ---";
                Console.WriteLine("\n{0}\n\nError: {1}, Connections: {2}, Gates: {3}, Genes: {4}, Time: {5}",
                        state,
                        plannedNetwork.TestResult.GetErrors().Sum(),
                        plannedNetwork.ConnectionCount,
                        plannedNetwork.NodeCount,
                        plannedNetwork.Plan.Genes.Count,
                        sw.Elapsed.TotalSeconds);
                //Console.WriteLine("Connections: {0}", GetConnectionsAsString(network));
                //Console.WriteLine("Nodes: {0}", GetNodesAsString(network));
            }
        }

        private static string GetNodesAsString(LogicalNetwork network)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in network.ConnectedNodes.Select(cn => cn.Node))
            {
                if (sb.Length != 0) sb.Append(',');
                sb.AppendFormat("{0}:{1}", node.Index, GetNodeName(node));
            }
            return sb.ToString();
        }

        private static string GetNodeName(Node node)
        {
            var gate = node as LogicGate;
            if (gate != null)
            {
                return gate.Type.ToString();
            }
            else
            {
                return "Network";
            }
        }

        private static string GetConnectionsAsString(LogicalNetwork network)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var conn in network.GetConnections())
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.AppendFormat("{0},{1}", conn.Index.UpperNodeIndex, conn.Index.LowerNodeIndex);
            }
            return sb.ToString();
        }
    }
}
