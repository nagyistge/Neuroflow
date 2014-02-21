using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImgNoise.Features;
using NeoComp.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Core;
using System.Xml;
using System.Runtime.Serialization;

namespace ImgNoise.Training
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
            bool recurrent = false;

            var trainingProv = CreateProvider(10000, recurrent);
            var trainingStrat = new GaussianBatchingStrategy(.5);
            //var trainingStrat = new MonteCarloBatchingStrategy();
            var trainingBatcher = new ScriptCollectionBatcher(trainingStrat, trainingProv, 200, 500);

            var validProv = CreateProvider(1000, recurrent);
            var validStrat = new MonteCarloBatchingStrategy();
            var validBatcher = new ScriptCollectionBatcher(validStrat, validProv, 25, 10000);

            trainingBatcher.Initialize();
            validBatcher.Initialize();

            Console.WriteLine("Training samples: " + trainingProv.Count);
            Console.WriteLine("Validation samples: " + validProv.Count);

            // Rules:
            Console.WriteLine("Creating learning rules ...");
            var weightInitRule = new NoisedWeightInitializationRule { Noise = 0.5, IsEnabled = true };
            //var learningRule = new QuickpropRule { StepSize = 0.001 };
            var learningRule = new SCGRule();
            //var learningRule = new LMRule();
            //var learningRule = new MetaQSARule { Mode = LearningMode.Stochastic, Momentum = 0.1, StepSizeRange = new DoubleRange(0.0, 0.005), StepSize = 0.001, StochasticAdaptiveStateUpdate = true };
            //var learningRule = new SuperSABRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSizeRange = new DoubleRange(0.0, 0.01), StepSize = 0.005, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new SignChangesRule { Mode = LearningMode.Stochastic, Momentum = 0.2, StepSizeRange = new DoubleRange(0.0, 0.001), StepSize = 0.001, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new GradientDescentRule { Mode = LearningMode.Stochastic, Momentum = 0.2, StepSize = 0.001 };
            //var learningRule = new QSARule();
            //var learningRule = new MAQRule();
            //var learningRule = new AdaptiveAnnealingRule { WeightGenMul = 0.1, AcceptProbMul = 0.05 };
            //var learningRule = new RpropRule { Momentum = 0.01, StepSize = 0.01 };
            //var learningRule = new CrossEntropyRule { PopulationSize = 50, NumberOfElites = 10, MutationChance = 0.01, MutationStrength = 0.01, DistributionType = DistributionType.Gaussian };
            //var learningRule = new GARule { PopulationSize = 40, MutationStrength = 0.01, MutationChance = 0.01 };

            var wdRule = (ILearningRule)learningRule as IWeightDecayedLearningRule;
            if (wdRule != null)
            {
                wdRule.WeightDecay = new WeightDecay { Factor = -0.0001, IsEnabled = false };
            }

            IterationRepeatPars iterationRepeatPars = new IterationRepeatPars(5, 10);

            // Net:
            Console.WriteLine("Creating Neural Network ...");
            var network = CreateNetwork(recurrent, weightInitRule, learningRule);
            var exec = new LearningExecution(network, iterationRepeatPars);

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

                //WriteResult(epoch);

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
                    }
                }
            }
            while (!done);
        }

        private static void Save(NeuralNetwork network)
        {
            Console.WriteLine("Saving ...");
            string fn = string.Format("comp {0}-{1}_{2}-{3}-{4}.xml", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
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

        static NeuralNetwork CreateNetwork(bool recurrent, params ILearningRule[] rules)
        {
            Contract.Requires(rules != null);
            Contract.Requires(rules.Length > 0);

            int inputSize;
            
            if (!recurrent)
            {
                inputSize = Properties.Settings.Default.SampleSize * Properties.Settings.Default.SampleSize + 1;
            }
            else
            {
                inputSize = Properties.Settings.Default.SampleSize + 1;
            }

            IFactory<Synapse> synapseFactory = new Factory<Synapse>(() => new Synapse(rules));
            IFactory<ActivationNeuron> neuronFactorySig = new Factory<ActivationNeuron>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05), rules));
            IFactory<ActivationNeuron> neuronFactoryLin = new Factory<ActivationNeuron>(() => new ActivationNeuron(new LinearActivationFunction(1.05), rules));

            //INeuralArchitecture arch = new LayeredNeuralArchitecture(
            //    new NeuralMLABuilder(
            //        false,
            //        new ConnectionLayerDefinition<double>(synapseFactory),
            //        inputSize,
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 20),
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 10),
            //        new OperationNodeLayerDefinition<double>(neuronFactoryLin, 1)));

            INeuralArchitecture arch = new WiredNeuralArchitecture(inputSize, 1, Properties.Settings.Default.NeuronCount, neuronFactorySig, neuronFactoryLin, synapseFactory, recurrent);

            //INeuralArchitecture arch = new WiredNeuralArchitecture(3, 1, 2, neuronFactorySig, neuronFactoryLin, synapseFactory, recurrent);

            return arch.CreateNetwork();
        }

        static ScriptCollectionProvider CreateProvider(int sampleCount, bool recurrent)
        {
            //string debugPath = @"C:\Users\unbornchikken\Pictures\NN\HQ_Parts\";
            //var searchPars = new SearchingParams(debugPath, new[] { "*.jpg", "*.png" }, true);
            
            var searchPars = new SearchingParams(Properties.Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Properties.Settings.Default.SampleSize, 25, 10);
            if (!recurrent)
            {
                return new NIPScriptProvider(searchPars, samplPars, sampleCount);
            }
            else
            {
                return new NILScriptProvider(searchPars, samplPars, sampleCount);            
            }
        }
    }
}
