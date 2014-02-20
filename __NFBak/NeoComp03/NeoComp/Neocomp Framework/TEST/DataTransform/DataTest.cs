using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Features;
using NeoComp.Computations;
using NeoComp.Core;
using System.Configuration;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Networks.Computational;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NeoComp.Optimization;
using NeoComp.Optimization.Learning;
using NeoComp.Optimization.Algorithms.Selection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml;

namespace TEST.DataTransform
{
    //public class TEST01Provider : EntityFeatureProvider<TEST01>
    //{
    //    public static readonly string[] InputIDs = new[] { "Vitality", "Time", "Strike" };

    //    public static readonly string[] OutputIDs = new[] { "Price" };

    //    public TEST01Provider()
    //        : base(ConfigurationManager.ConnectionStrings["NeoCompEntities"].ConnectionString, "NeoCompEntities.TEST01", "ID", new[] { "Vitality", "Time", "Strike", "Price" })
    //    {
    //    }
    //}

    public class TEST01Provider : ListFeatureProvider
    {
        public static readonly string[] InputIDs = new[] { "VLTY", "TIME", "STRIKE" };

        public static readonly string[] OutputIDs = new[] { "OPRICE" };

        public TEST01Provider()
            : base(new TextItemReader(new StreamReader(@"c:\Users\unbornchikken\Documents\Peltarion Synapse\Sample Data\data.txt")), new[] { "VLTY", "TIME", "STRIKE", "OPRICE" })
        {
        }
    }

    public class PoliceProvider : EntityFeatureProvider<Police>
    {
        public static readonly string[] InputIDs = new[] { "Age", "AvG", "Chdn", "ExEd", "CR", "SexIsF", "SecE", "AvgE" };

        public static readonly string[] OutputIDs = new[] { "FinalE" };

        public PoliceProvider()
            : base(ConfigurationManager.ConnectionStrings["NeoCompEntities"].ConnectionString, "NeoCompEntities.Police", "ID", new[] { "Age", "AvG", "Chdn", "ExEd", "CR", "SexIsF", "SecE", "AvgE", "FinalE>" })
        {
        }
    }
    
    public static class DataTest
    {
        public static void Begin()
        {
            //TestComputation("comp.xml");
            //return;

            Console.WriteLine("Press any key to start ...");
            Console.ReadKey();
            
            // Data:
            var rootDataProvider = new TEST01Provider();

            //var factory = new SubsetValidatorFactory(rootDataProvider, new[] { "it.ID > 100" }, new[] { "it.ID <= 100" });
            //var factory = new SubsetValidatorFactory(rootDataProvider,
            //    new ListFeatureFilter((rowIdx, row) => row["FCTNO"] > 229),
            //    new ListFeatureFilter((rowIdx, row) => row["FCTNO"] <= 229));
            var factory = new ClientValidatorFactory(rootDataProvider, 229);

            IDataFeatureProvider testDataProvider, validationDataProvider;
            factory.GetProviders(out testDataProvider, out validationDataProvider);

            // MAQ: 20 - 5000
            // QSA: 50 - 100

            //var selStrat = new MonteCarloDataFeatureSelectionStrategy(25, MonteCarloMode.NewBlock);
            //var selStrat = new AllDataFeatureSelectionStrategy();
            var selStrat = new IntelligentDataFeatureSelectionStrategy(100, new GaussianSelectionAlgorithm(), 5000, new MTPEliminationParameters());
            var valSelStrat = new AllDataFeatureSelectionStrategy();
            var matrixProvider = new SupervisedDataFeatureMatrixProvider(selStrat, testDataProvider, TEST01Provider.InputIDs, TEST01Provider.OutputIDs);
            var validationMatrixProvider = new SupervisedDataFeatureMatrixProvider(valSelStrat, validationDataProvider, TEST01Provider.InputIDs, TEST01Provider.OutputIDs);

            // Net:
            var decay = new WeightDecayRule { IsEnabled = false, Factor = -0.0005, UpdateOnEachVector = false };
            var weightInitRule = new NoisedWeightInitializationRule { Noise = 1 };
            //var learningRule = new GradientDescentRule { Mode = LearningMode.Stochastic, Momentum = 0.4, StepSize = 0.1 };
            //var learningRule = new QuickpropRule { Momentum = 0.8, StepSize = 0.05 };
            //var learningRule = new SignChangesRule { Mode = LearningMode.Stochastic, Momentum = 0.8, StepSizeRange = new DoubleRange(0.001, 0.1), StepSize = 0.1, StochasticAdaptiveStateUpdate = false };
            var learningRule = new SuperSABRule { Mode = LearningMode.Stochastic, Momentum = 0.1, StepSizeRange = new DoubleRange(0.01, 0.2), StepSize = 0.1, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new RpropRule { Mode = LearningMode.Batch, Momentum = 0.8, StepSize = 0.01, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new SCGRule();
            //var learningRule = new MetaQSARule { Mode = LearningMode.Stochastic, Momentum = 0.1, StepSizeRange = new DoubleRange(0.005, 0.3), StepSize = 0.2, StochasticAdaptiveStateUpdate = false };
            //var learningRule = new QSARule();
            //var learningRule = new MAQRule();
            //var learningRule = new LMRule();

            IFactory<Synapse> synapseFactory = new Factory<Synapse>(() => new Synapse(decay, weightInitRule, learningRule));
            IFactory<ActivationNeuron> neuronFactorySig = new Factory<ActivationNeuron>(() => new ActivationNeuron(new SigmoidActivationFunction(1.7), decay, weightInitRule, learningRule));
            IFactory<ActivationNeuron> neuronFactoryLin = new Factory<ActivationNeuron>(() => new ActivationNeuron(new LinearActivationFunction(1.05), decay, weightInitRule, learningRule));

            INeuralArchitecture arch = new LayeredNeuralArchitecture(
                new NeuralMLABuilder(
                    false,
                    new ConnectionLayerDefinition<double>(synapseFactory),
                    matrixProvider.MatrixWidth,
                    new OperationNodeLayerDefinition<double>(neuronFactorySig, 6),
                    new OperationNodeLayerDefinition<double>(neuronFactorySig, 4),
                    new OperationNodeLayerDefinition<double>(neuronFactoryLin, matrixProvider.OutputMatrixWidth)));

            //INeuralArchitecture arch = new WiredNeuralArchitecture(matrixProvider.MatrixWidth, matrixProvider.OutputMatrixWidth, 7, neuronFactorySig, neuronFactoryLin, synapseFactory);

            var network = arch.CreateNetwork();

            var items = network.GetItems();
            int nc = items.NodeEntries.Count();
            int cc = items.ConnectionEntries.Where(ce => ce.Connection is Synapse).Count() + nc;
            Console.WriteLine("Conns: " + cc + " Nodes: " + nc);

            var learning = new Learning(network);
            var epoch = new OptimizationEpoch(learning, matrixProvider, validationMatrixProvider, 1);
            epoch.Initialize();

            bool done = false;
            NeuralComputation resultComp = null;

            epoch.BestValidationResult.Updated += (sender, e) =>
            {
                var result = (OptimizationResult)sender;
                if (result.Epoch.CurrentValidationResult.MSE < 0.0001 && result.Epoch.CurrentResult.MSE < 0.0001)
                {
                    var neuralComp = new NeuralComputation((Learning)result.Epoch.Unit, result.Epoch.MatrixProvider);
                    resultComp = neuralComp.Clone();
                }
            };

            while (!done)
            {
                epoch.Step();
                if (epoch.CurrentIteration % 10 == 0)
                {
                    Console.WriteLine("{0} - Current: {1}/{2} Validation: {3}/{4} {5}",
                        epoch.CurrentIteration,
                        epoch.BestResult.MSE.ToString("0.000000"),
                        epoch.CurrentResult.MSE.ToString("0.000000"),
                        epoch.BestValidationResult.MSE.ToString("0.000000"),
                        epoch.CurrentValidationResult.MSE.ToString("0.000000"),
                        resultComp == null ? "" : "!");
                }

                if (Console.KeyAvailable)
                {
                    Console.ReadKey();
                    done = true;
                }
            }

            if (resultComp != null)
            {
                SaveComputation(resultComp, "comp.xml");
                TestComputation("comp.xml");
            }
        }

        private static void SaveComputation(NeuralComputation comp, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                var dcs = new DataContractSerializer(typeof(NeuralComputation));
                var s = new XmlWriterSettings();
                s.Indent = true;
                var xml = XmlWriter.Create(writer, s);
                dcs.WriteObject(xml, comp);
                xml.Flush();
                writer.Flush();
            }
        }

        private static void TestComputation(string fileName)
        {
            NeuralComputation comp;
            
            using (var file = File.OpenText(fileName))
            {
                var dcs = new DataContractSerializer(typeof(NeuralComputation));
                comp = (NeuralComputation)dcs.ReadObject(XmlReader.Create(file));
            }

            // 2; 5; 100; 11.2019
            var result = comp.Compute(2, 5, 100);
            Console.WriteLine(result[0]);
        }
    }
}
