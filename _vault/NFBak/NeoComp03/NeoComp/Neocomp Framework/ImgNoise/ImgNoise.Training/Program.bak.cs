using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImgNoise.Features;
using NeoComp.Core;
using NeoComp.Features;
using NeoComp.Optimization.Algorithms.Selection;
using NeoComp.Optimization.Learning;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational;
using NeoComp.Optimization;
using System.Xml;
using System.Runtime.Serialization;
using NeoComp.DEBUG;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImgNoise.Training
{
    class Program
    {
        static NeuralNetwork trainedNet = null;
        
        static void Main(string[] args)
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
            // Data:
            
            Console.WriteLine("Creating data providers ...");

            var rootDataProv = CreateDataProvider(20000);
            var valDataProv = rootDataProv.GetObjectSubsetProvider((index, values) => index < 5000);
            var trainingDataProv = rootDataProv.GetObjectSubsetProvider((index, values) => index >= 5000);

            //var trainingSelStrat = new IntelligentDataFeatureSelectionStrategy(250, new GaussianSelectionAlgorithm(), 2500, new MTPEliminationParameters(5, 10));
            var trainingSelStrat = new MonteCarloDataFeatureSelectionStrategy(250, MonteCarloMode.ReplaceOneItem);
            var valSelStrat = new MonteCarloDataFeatureSelectionStrategy(250, MonteCarloMode.NewBlock);

            var trainingMP = new SupervisedDataFeatureMatrixProvider(trainingSelStrat, trainingDataProv, NIPDataProvider.InputFeatureIDs, NIPDataProvider.OutputFeatureIDs);
            var valMP = new SupervisedDataFeatureMatrixProvider(valSelStrat, valDataProv, NIPDataProvider.InputFeatureIDs, NIPDataProvider.OutputFeatureIDs);

            Console.WriteLine("Samples found: " + rootDataProv.ItemCount);
            Console.WriteLine("Training samples: " + trainingDataProv.ItemCount);
            Console.WriteLine("Validation samples: " + valDataProv.ItemCount);

            // Rules:
            Console.WriteLine("Creating learning rules ...");
            var weightInitRule = new NoisedWeightInitializationRule { Noise = 1.0, IsEnabled = true };
            var decayRule = new WeightDecayRule { Factor = -0.00001, IsEnabled = true };
            //var learningRule = new QuickpropRule { StepSize = 0.001 };
            var learningRule = new SCGRule();
            //var learningRule = new LMRule();
            //var learningRule = new MetaQSARule { Mode = LearningMode.Stochastic, Momentum = 0.8, StepSizeRange = new DoubleRange(0.0, 0.005), StepSize = 0.001, StochasticAdaptiveStateUpdate = true };
            //var learningRule = new SuperSABRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSizeRange = new DoubleRange(0.0, 0.5), StepSize = 0.1, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new SignChangesRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSizeRange = new DoubleRange(0.0, 0.05), StepSize = 0.01, StochasticAdaptiveStateUpdate = true };
            //var learningRule = new GradientDescentRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSize = 0.1 };
            //var learningRule = new QSARule();
            //var learningRule = new MAQRule();
            //var learningRule = new RpropRule { Momentum = 0.01, StepSize = 0.01 };
            //var learningRule = new CrossEntropyRule { PopulationSize = 400, NumberOfElites = 100 };
            //var learningRule = new GARule { };

            // Net:
            Console.WriteLine("Creating Neural Network ...");
            var network = CreateNetwork(trainingMP, weightInitRule, decayRule, learningRule);
            var learning = new Learning(network);

            // Epoch:
            Console.WriteLine("Initializing optimization epoch ...");
            var epoch = new OptimizationEpoch(learning, trainingMP, valMP, 5);
            epoch.Initialize();

            epoch.BestValidationResult.Updated += OnBestResultUpdated;

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

                WriteResult(epoch, trainedNet != null);
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                            done = true;
                            break;
                        case ConsoleKey.S:
                            Save(network, trainingMP);
                            break;
                        case ConsoleKey.V:
                            if (trainedNet != null) Save(trainedNet, trainingMP);
                            break;
                    }
                }
            }
            while (!done);
        }

        private static void Save(NeuralNetwork network, IFeaturedInputOutput featuredObject)
        {
            Console.WriteLine("Saving ...");
            var comp = new NeuralComputation(trainedNet, featuredObject: featuredObject);
            string fn = string.Format("comp {0}-{1}_{2}-{3}-{4}.xml", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            using (var xml = XmlWriter.Create(fn, new XmlWriterSettings { Indent = true }))
            {
                var s = new DataContractSerializer(typeof(NeuralComputation));
                s.WriteObject(xml, comp);
                xml.Flush();
            }
        }

        private static void WriteResult(OptimizationEpoch epoch, bool trained)
        {
            Console.WriteLine("{0}: Current: {1}/{2} Validation: {3}/{4} {5}",
                        epoch.CurrentIteration.ToString("0000"),
                        epoch.BestResult.MSE.ToString("0.000000"),
                        epoch.CurrentResult.MSE.ToString("0.000000"),
                        epoch.BestValidationResult.MSE.ToString("0.000000"),
                        epoch.CurrentValidationResult.MSE.ToString("0.000000"),
                        trained ? "!" : "");
        }

        static void OnBestResultUpdated(object sender, OptimizationResultUpdatedEventArgs e)
        {
            trainedNet = ((Learning)e.Epoch.Unit).Network.Clone();
        }

        static NeuralNetwork CreateNetwork(ISupervisedFeatureMatrixProvider matrixProv, params ILearningRule[] rules)
        {
            Contract.Requires(matrixProv != null);
            Contract.Requires(rules != null);
            Contract.Requires(rules.Length > 0);

            IFactory<Synapse> synapseFactory = new Factory<Synapse>(() => new Synapse(rules));
            IFactory<ActivationNeuron> neuronFactorySig = new Factory<ActivationNeuron>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05), rules));
            IFactory<ActivationNeuron> neuronFactoryLin = new Factory<ActivationNeuron>(() => new ActivationNeuron(new LinearActivationFunction(1.05), rules));

            //INeuralArchitecture arch = new LayeredNeuralArchitecture(
            //    new NeuralMLABuilder(
            //        false,
            //        new ConnectionLayerDefinition<double>(synapseFactory),
            //        matrixProv.MatrixWidth,
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 20),
            //        new OperationNodeLayerDefinition<double>(neuronFactorySig, 10),
            //        new OperationNodeLayerDefinition<double>(neuronFactoryLin, matrixProv.OutputMatrixWidth)));

            INeuralArchitecture arch = new WiredNeuralArchitecture(matrixProv.MatrixWidth, matrixProv.OutputMatrixWidth, Properties.Settings.Default.NeuronCount, neuronFactorySig, neuronFactoryLin, synapseFactory);

            return arch.CreateNetwork();
        }

        static NIPDataProvider CreateDataProvider(int max)
        {
            var searchPars = new SearchingParams(Properties.Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Properties.Settings.Default.SampleSize);
            var prov = new NIPDataProvider(searchPars, samplPars, max);
            return prov;
        }
    }
}
