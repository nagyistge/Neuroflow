using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Core;
using NeoComp.Optimization.GeneticNetworks;
using NeoComp.Optimization.GA;
using NeoComp.Networks;

namespace TEST.Neural
{
    public static class NeuralGATest
    {
        const string problem =
            //@"
            //-5.0 = 25.0
            //-4.0 = 16.0
            //-3.0 = 9.0
            //-2.0 = 4.0
            //-1.0 = 1.0
            //0.0 = 0.0
            //1.0 = 1.0
            //2.0 = 4.0
            //3.0 = 9.0
            //4.0 = 16.0
            //5.0 = 25.0
            //";

//@"
            //0.0 0.0 0.0 = 0.0 0.0
            //0.0 0.0 1.0 = 0.0 1.0
            //0.0 1.0 0.0 = 0.0 1.0
            //0.0 1.0 1.0 = 1.0 0.0
            //1.0 0.0 0.0 = 0.0 1.0
            //1.0 0.0 1.0 = 1.0 0.0
            //1.0 1.0 0.0 = 1.0 0.0
            //1.0 1.0 1.0 = 1.0 1.0
            //";

@"
1 = 0
2 = 4
3 = 7
4 = 9
5 = 10
6 = 9
7 = 7
8 = 4
9 = 0
";

        static NeuralNetworkTest test = NeuralNetworkTest.Create(problem);

        const int groupCount = 25;

        const int groupSize = 25;

        public static void Begin()
        {
            var parameters = new ConfiguredNeuralNetworkParameters
            {
                InputInterfaceLength = test.InputSize,
                OutputInterfaceLength = test.OutputSize,
                MaxConnectionIndex = 24,                             
                ConnectionCountRange = IntRange.CreateInclusive(36, 72),
                FeedForward = true,                           
                ActivationFunction = new LinearActivationFunction(2.0),
                Test = test,
                FunctionalErrorTreshold = 0.001
            };

            var factory = new ConfiguredNeuralNetworkBodyFactory(parameters);

            factory.MutationParameters.MutationChunkSize = IntRange.CreateFixed(parameters.ConnectionCountRange.MaxValue);
            factory.MutationParameters.CrossoverChunkSize = IntRange.CreateFixed(parameters.ConnectionCountRange.MaxValue);

            factory.MutationParameters.PointMutationChance = 0.04;

            factory.MutationParameters.DeletionMutationChance = 0.002;
            factory.MutationParameters.DuplicationMutationChance = 0.002;
            factory.MutationParameters.InsertionMutationChance = 0.002;
            factory.MutationParameters.TranslocationMutationChance = 0.002;
            factory.MutationParameters.InversionMutationChance = 0.002;

            var population = new ConfiguredNeuralNetworkPopulation();
            population.ChanceOfMigration = 0.1;

            for (int idx = 0; idx < groupCount; idx++)
            {
                var group = new ConfiguredNeuralNetworkGroup(
                    factory, 
                    groupSize);
                //group.SelectionStrategy = new RandomSelectionStrategy();
                population.Groups.Add(group);
            }

            var context = new GAContext(population);
            context.BestBodyArrived += OnBestArrived;
            context.Start();
            Console.ReadKey();
            context.Stop();
        }

        static readonly object cl = new object();

        static void OnBestArrived(object sender, BestBodyArrivedToPopulationEventArgs e)
        {
            lock (cl)
            {
                var plannedNetwork = (ConfiguredNeuralNetworkBody)e.Body;
                var network = plannedNetwork.CreateNetwork();
                string state = plannedNetwork.IsFunctional ? "*** PASSED ***" : "--- BEST ---";
                Console.WriteLine("\n{0}\n\nError: {1}, Synapses: {2}, Neurons: {3}, Genes: {4}",
                        state,
                        plannedNetwork.Error,
                        plannedNetwork.ConnectionCount,
                        plannedNetwork.NodeCount,
                        plannedNetwork.Plan.Genes.Count);
                Console.WriteLine("Connections: {0}", GetConnectionsAsString(network));
                Console.WriteLine("Nodes: {0}", GetNodesAsString(network));
            }
        }

        private static string GetNodesAsString(NeuralNetwork network)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in network.ConnectedNodes.Select(cn => cn.Node))
            {
                if (sb.Length != 0) sb.Append(',');
                sb.AppendFormat("{0}:{1}", node.Index, NodeAsString(node));
            }
            return sb.ToString();
        }

        private static string NodeAsString(Node node)
        {
            var neuron = node as ActivationNeuron;
            if (neuron != null)
            {
                return neuron.Bias.ToString("0.00");
            }
            else
            {
                return "Network";
            }
        }

        private static string GetConnectionsAsString(NeuralNetwork network)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var conn in network.GetConnections())
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.AppendFormat("[{0},{1} {2}]", conn.Index.UpperNodeIndex, conn.Index.LowerNodeIndex, Math.Round(conn.Weight, 2));
            }
            return sb.ToString();
        }
    }
}
