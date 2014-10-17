using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.Learning;
using NeoComp.Core;
using System.Diagnostics;

namespace TEST.Learning
{
    public enum Algorithm { GA, QSA, BP, MSEAQA, CompMSEAQA }
    
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

        public static void Begin()
        {
            Algorithm algorihtm = Algorithm.BP;
            
            NeuralNetworkTest test = null;
            Func<Synapse> synapseFactory = null;
            Func<Neuron> neuronFactory = null;
            ISupervisedLearningEpoch epoch = null;

            switch (algorihtm)
            {
                case Algorithm.BP:
                    test = NeuralNetworkTest.Create(problem, true);
                    synapseFactory = () => new BackPropagationSynapse();
                    neuronFactory = () => new BackPropagationNeuron(new BipolarSigmoidActivationFunction(2));
                    break;
                default:
                    test = NeuralNetworkTest.Create(problem);
                    synapseFactory = () => new Synapse();
                    neuronFactory = () => new ActivationNeuron(new LinearActivationFunction(1));
                    break;
            }

            var network = NeuralArchitecture.CreateLayered(
                           test.InputSize,
                           test.OutputSize,
                           synapseFactory,
                           neuronFactory,
                           true,
                           5);

            //var network = NeuralArchitecture.CreateFullConnected(
            //               test.InputSize,
            //               test.OutputSize,
            //               6,
            //               synapseFactory,
            //               neuronFactory,
            //               true);

            switch (algorihtm)
            {
                case Algorithm.BP:
                    var bpStrategy = new BackPropagationLearningStrategy(network, 0.1, 0.1);
                    epoch = new BackPropagationLearningEpoch(bpStrategy, test);
                    break;
                case Algorithm.GA:
                    epoch = new GALearningEpoch(network, test);
                    break;
                case Algorithm.QSA:
                    var qsaStrategy = new SupervisedQSALearningStrategy(network, 0.1, 0.85, 0.05, 0.0001);
                    epoch = new SupervisedQSALearningEpoch(qsaStrategy, test);
                    break;
                case Algorithm.MSEAQA:
                    epoch = new MSEAQALearningEpoch(network, test);
                    break;
                case Algorithm.CompMSEAQA:
                    epoch = new CompetitiveMSEAQALearningEpoch(new CompetitiveMSEAQALearningStrategy(network), test);
                    break;
            }

            epoch.Initialize();
            epoch.Step();

            var sw = new Stopwatch();
            double errorTreshold = 0.000000;
            double currentError = 1.0;
            do
            {
                //if (Console.KeyAvailable)
                //{
                //    Console.ReadKey();
                //    break;
                //}

                sw.Start();
                var currentResult = epoch.Step();
                sw.Stop();
                currentError = currentResult.MSE;
                if (epoch.Iteration % 1000 == 0 || currentError <= errorTreshold)
                {
                    Console.WriteLine("MSE: {0} Time: {1} Iteration: {2}",
                        currentError.ToString(),
                        sw.Elapsed.TotalSeconds.ToString("0.00"),
                        epoch.Iteration);
                }
            }
            while (currentError > errorTreshold);
        }
    }
}
