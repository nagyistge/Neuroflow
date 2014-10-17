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
    public static class AdjustedGATest
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

@"
0.0 0.0 0.0 = 0.0 0.0
0.0 0.0 1.0 = 0.0 1.0
0.0 1.0 0.0 = 0.0 1.0
0.0 1.0 1.0 = 1.0 0.0
1.0 0.0 0.0 = 0.0 1.0
1.0 0.0 1.0 = 1.0 0.0
1.0 1.0 0.0 = 1.0 0.0
1.0 1.0 1.0 = 1.0 1.0
";

        static NeuralNetworkTest test = NeuralNetworkTest.Create(problem);

        const int groupCount = 5;

        const int groupSize = 25;

        public static void Begin()
        {
            //var parameters = new AdjustedTestableNetworkParameters
            //{
            //    Test = test,
            //    Network = NeuralArchitecture.CreateLayered(
            //        test.InputSize,
            //        test.OutputSize,
            //        () => new Synapse(),
            //        () => new ActivationNeuron(new LinearActivationFunction(1)),
            //        true,
            //        20, 10)
            //};

            var parameters = new AdjustedTestableNetworkParameters
            {
                Test = test,
                Network = NeuralArchitecture.CreateFullConnected(
                                test.InputSize,
                                test.OutputSize,
                                20,
                                () => new Synapse(),
                                () => new ActivationNeuron(new LinearActivationFunction(1)),
                                true)
            };

            var factory = new AdjustedNeuralNetworkBodyFactory(parameters);

            factory.CrossoverChunkSize = IntRange.CreateFixed((parameters.Network.ConnectionCount + parameters.Network.NodeCount) / 2 - 1);
            //factory.CrossoverChunkSize = new InclusiveRange(10, 10);

            factory.PointMutationChance = 0.005;

            var population = new AdjustedNeuralNetworkPopulation();
            population.ChanceOfMigration = 0.1;

            for (int idx = 0; idx < groupCount; idx++)
            {
                var group = new AdjustedNeuralNetworkGroup(
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
                var plannedNetwork = (AdjustedNeuralNetworkBody)e.Body;
                string state = plannedNetwork.Error <= 0.0001 ? "*** PASSED ***" : "--- BEST ---";
                Console.WriteLine("\n{0}\n\nError: {1}",
                        state,
                        plannedNetwork.Error.ToString("0.000000"));
                Console.WriteLine(
                    plannedNetwork.TestResult.Computation.ComputeOutput(
                    (NeuralNetwork)plannedNetwork.Parameters.Network,
                    new List<double> { 4.4 })[0]); // 19.36
            }
        }
    }
}
