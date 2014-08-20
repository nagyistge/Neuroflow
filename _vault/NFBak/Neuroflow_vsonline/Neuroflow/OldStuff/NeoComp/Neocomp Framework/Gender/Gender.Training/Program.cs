using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Learning;
using Gender.Data;
using NeoComp.Core;
using System.Runtime.Serialization;
using System.Xml;
using NeoComp.Networks.Computational;

namespace Gender.Training
{
    internal static class Program
    {
        static NeuralNetwork vbestNet = null;

        static void Main()
        {
            try
            {
                Begin();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.GetType().Name);
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static void Begin()
        {
            var trainingProv = CreateProvider(true, 10000);
            var trainingStrat = new GaussianBatchingStrategy(2.0);
            //var trainingStrat = new MonteCarloBatchingStrategy();
            var trainingBatcher = new ScriptCollectionBatcher(trainingStrat, trainingProv, 250, 250);

            var validProv = CreateProvider(false, 1000);
            var validStrat = new MonteCarloBatchingStrategy();
            var validBatcher = new ScriptCollectionBatcher(validStrat, validProv, 50, 1000);

            trainingBatcher.Initialize();
            validBatcher.Initialize();

            Console.WriteLine("Training samples: " + trainingProv.Count);
            Console.WriteLine("Validation samples: " + validProv.Count);

            // Rules:
            Console.WriteLine("Creating learning rules ...");
            var weightInitRule = new NoisedWeightInitializationRule { Noise = 0.5, IsEnabled = true };
            var decayRule = new WeightDecayRule { Factor = -0.000001, IsEnabled = false };
            //var learningRule = new QuickpropRule { StepSize = 0.01 };
            //var learningRule = new SCGRule();
            //var learningRule = new LMRule();
            //var learningRule = new MetaQSARule { Mode = LearningMode.Stochastic, Momentum = 0.1, StepSizeRange = new DoubleRange(0.0, 0.01), StepSize = 0.005, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new SuperSABRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSizeRange = new DoubleRange(0.0, 0.05), StepSize = 0.01, StochasticAdaptiveStateUpdate = false };
            var learningRule = new SignChangesRule { Mode = LearningMode.Stochastic, Momentum = 0.2, StepSizeRange = new DoubleRange(0.0, 0.001), StepSize = 0.001, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new GradientDescentRule { Mode = LearningMode.Stochastic, Momentum = 0.1, StepSize = 0.0001 };
            //var learningRule = new QSARule();
            //var learningRule = new MAQRule();
            //var learningRule = new AdaptiveAnnealingRule { WeightGenMul = 0.01, AcceptProbMul = 0.01 };
            //var learningRule = new RpropRule { Momentum = 0.01, StepSize = 0.0001 };
            //var learningRule = new CrossEntropyRule { PopulationSize = 50, NumberOfElites = 10, MutationChance = 0.001, MutationStrength = 0.1, DistributionType = DistributionType.Gaussian };
            //var learningRule = new GARule { };

            var wdRule = (ILearningRule)learningRule as IWeightDecayedLearningRule;
            if (wdRule != null)
            {
                wdRule.WeightDecay = new WeightDecay { Factor = -0.0001, IsEnabled = false };
            }

            IterationRepeatPars iterationRepeatPars = new IterationRepeatPars(1, 5);

            // Net:
            Console.WriteLine("Creating Neural Network ...");
            var network = CreateNetwork(trainingProv.InputSize, trainingProv.OutputSize, weightInitRule, decayRule, learningRule);
            var exec = new LearningExecution(network, iterationRepeatPars);
            var items = network.GetItems();
            int nc = items.NodeEntries.Select(e => e.Node).OfType<ActivationNeuron>().Count();
            int sc = items.ConnectionEntries.Select(e => e.Connection).OfType<Synapse>().Count();
            Console.WriteLine("Neurons: {0} Synapses: {1}", nc, sc);

            // Epoch:
            Console.WriteLine("Initializing epoch ...");
            var epoch = new LearningEpoch(exec, trainingBatcher, validBatcher, 1);
            epoch.Initialize();
            epoch.CurrentResult.Updated += (sender, e) => WriteResult(epoch);
            epoch.BestValidationResult.Updated += (sender, e) => vbestNet = network.Clone();

            // Training loop:
            Console.WriteLine("Starting ...");

            bool done = false;
            do
            {
                //CodeBench.By("Epoch").Do = () =>
                //{
                //    epoch.Step();
                //};

                //CodeBench.By("Epoch").WriteToConsole();

                epoch.Step();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                            done = true;
                            break;
                        case ConsoleKey.S:
                            Save(network.Clone());
                            break;
                        case ConsoleKey.V:
                            if (vbestNet != null) Save(vbestNet);
                            break;
                        case ConsoleKey.NumPad1:
                            Test(network.Clone());
                            break;
                        case ConsoleKey.NumPad2:
                            if (vbestNet != null) Test(vbestNet);
                            break;
                    }
                }
            }
            while (!done);
        }

        private static void Test(NeuralNetwork network)
        {
            var comp = new GenderComputation(network, Properties.Settings.Default.NumberOfIterations);
            var test = new GenderComputationTester(comp, GenderTestSet.Validation);
            test.Update();
            Console.WriteLine("\nCount: {0} Passed: {1} Failed: {2}", test.TestedItems, test.PassedItems, test.FailedItems);
            Console.WriteLine("Ratio: {0}\n", Math.Round(test.PassedRatio * 100.0, 4));
            Console.ReadKey();
        }

        private static void Save(NeuralNetwork network)
        {
            Console.WriteLine("Saving ...");
            var now = DateTime.Now;
            string fn = string.Format("comp {0}-{1}_{2}-{3}-{4}.xml",
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));
            using (var xml = XmlWriter.Create(fn, new XmlWriterSettings { Indent = true }))
            {
                var s = new DataContractSerializer(typeof(NeuralNetwork));
                s.WriteObject(xml, network);
                xml.Flush();
            }
        }

        private static void WriteResult(LearningEpoch epoch)
        {
            Console.WriteLine("{0}: Current: {1}/{2} Validation: {3}/{4}",
                        epoch.CurrentIteration.ToString("0000"),
                        epoch.BestResult.MSE.ToString("0.000000"),
                        epoch.CurrentResult.MSE.ToString("0.000000"),
                        epoch.BestValidationResult.MSE.ToString("0.000000"),
                        epoch.CurrentValidationResult.MSE.ToString("0.000000"));
        }

        static NeuralNetwork CreateNetwork(int inputSize, int outputSize, params ILearningRule[] rules)
        {
            IFactory<Synapse> synapseFactory = new Factory<Synapse>(() => new Synapse(rules));
            IFactory<ActivationNeuron> neuronFactorySig = new Factory<ActivationNeuron>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05), rules));
            IFactory<ActivationNeuron> neuronFactoryLin = new Factory<ActivationNeuron>(() => new ActivationNeuron(new LinearActivationFunction(1.05), rules));

            //INeuralArchitecture arch = new LayeredNeuralArchitecture(
            //    new NeuralMLABuilder(
            //        false,
            //        new ConnectionLayerDefinition<double>(synapseFactory),
            //        inputSize,
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 10),
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 6),
            //        new OperationNodeLayerDefinition<double>(neuronFactoryLin, outputSize)));

            INeuralArchitecture arch = new WiredNeuralArchitecture(
                inputSize,
                outputSize,
                Properties.Settings.Default.NeuronCount,
                neuronFactorySig,
                neuronFactoryLin,
                synapseFactory,
                Properties.Settings.Default.NumberOfIterations != 1);

            return arch.CreateNetwork();
        }

        static GenderScriptProvider CreateProvider(bool isTraining, int sampleCount)
        {
            return new GenderScriptProvider(isTraining, sampleCount, true, Properties.Settings.Default.NumberOfIterations);
        }
    }
}
