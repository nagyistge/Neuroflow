using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Epoch;
using NeoComp.Core;
using NeoComp.Optimization.SupervisedLearning;
using NeoComp.Networks.Computational;

namespace TEST.Supervised
{
    public enum Algorithm { GA, QSA, BP, MAQ }

    public static class Test
    {
        const string problem =
        @"
        -5.0 = 25.0
        -4.0 = 16.0
        -3.0 = 9.0
        -2.0 = 4.0
        -1.0 = 1.0
        0.0 = 0.0
        1.0 = 1.0
        2.0 = 4.0
        3.0 = 9.0
        4.0 = 16.0
        5.0 = 25.0
        ";

//        @"
//        0.0 0.0 0.0 = 0.0 0.0
//        0.0 0.0 1.0 = 0.0 1.0
//        0.0 1.0 0.0 = 0.0 1.0
//        0.0 1.0 1.0 = 1.0 0.0
//        1.0 0.0 0.0 = 0.0 1.0
//        1.0 0.0 1.0 = 1.0 0.0
//        1.0 1.0 0.0 = 1.0 0.0
//        1.0 1.0 1.0 = 1.0 1.0
//        ";

        public static void Begin()
        {
            Algorithm algorihtm = Algorithm.MAQ;

            NeuralNetworkTest test = null;
            IFactory<Synapse> synapseFactory = null;
            IFactory<ActivationNeuron> neuronFactory = null;
            LearningEpoch epoch = null;

            switch (algorihtm)
            {
                case Algorithm.BP:
                    test = NeuralNetworkTest.Create(problem, true);
                    synapseFactory = new Factory<Synapse>();
                    neuronFactory = new Factory<ActivationNeuron>(() => new ActivationNeuron(new BipolarSigmoidActivationFunction(2)));
                    break;
                default:
                    test = NeuralNetworkTest.Create(problem);
                    synapseFactory = new Factory<Synapse>();
                    neuronFactory = new Factory<ActivationNeuron>(() => new ActivationNeuron(new LinearActivationFunction(1)));
                    break;
            }

            //INeuralArchitecture arch = new WiredNeuralArchitecture(test.InputSize, test.OutputSize, 15, neuronFactory, synapseFactory);

            INeuralArchitecture arch = new LayeredNeuralArchitecture(
                new NeuralMLABuilder(
                    false,
                    new ConnectionLayerDefinition<double>(synapseFactory),
                    test.InputSize,
                    new OperationNodeLayerDefinition<double>(neuronFactory, 20),
                    new OperationNodeLayerDefinition<double>(neuronFactory, test.OutputSize)));

            var network = arch.CreateNetwork();

            switch (algorihtm)
            {
                case Algorithm.BP:
                    epoch = new BackPropagationLearningEpoch(new GradientDescentRule(new GDRuleParameters()), network, test);
                    break;
                default:
                    epoch = new MAQLearningEpoch(network, test);
                    break;
            }

            epoch.Initialize();

            double errorTreshold = 0.000000;
            double currentError = 1.0;
            do
            {
                //if (Console.KeyAvailable)
                //{
                //    Console.ReadKey();
                //    break;
                //}

                epoch.Step();

                currentError = epoch.CurrentResult.MSE;

                if (epoch.CurrentIteration % 1000 == 0 || currentError <= errorTreshold)
                {
                    Console.WriteLine("MSE: {0} Time: {1} Iteration: {2}",
                        currentError.ToString(),
                        epoch.ElapsedTime.TotalSeconds.ToString("0.00"),
                        epoch.CurrentIteration);
                }
            }
            while (currentError > errorTreshold);
        }
    }
}
