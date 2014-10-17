using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Optimization.GeneticNetworks;
using NeoComp.Optimization.GA;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NeoComp.Optimization.Learning
{
    public sealed class GALearningStrategy : LearningStrategy, IParallelLearningStrategy
    {
        public GALearningStrategy(
            NeuralNetwork network,
            AdjustedNeuralNetworkPopulationFactory populationFactory = null)
            : base(network)
        {
            Contract.Requires(network != null);

            this.populationFactory = populationFactory == null ? 
                new AdjustedNeuralNetworkPopulationFactory() : 
                populationFactory;
        }

        #region Fields

        bool populationInitialized;

        AdjustedNeuralNetworkBody bestBody;

        AdjustedNeuralNetworkPopulation population;

        AdjustedNeuralNetworkBodyFactory bodyFactory;

        #endregion

        #region Properties

        AdjustedNeuralNetworkPopulationFactory populationFactory;

        public AdjustedNeuralNetworkPopulationFactory PopulationFactory
        {
            get { lock (SyncRoot) return populationFactory; }
        }

        #endregion

        #region Init New Run

        protected internal override void InitializeNewRun()
        {
            lock (Network.SyncRoot) lock (SyncRoot) DoInitNewRun();
        }

        private void DoInitNewRun()
        {
            if (population != null) RemoveBestHandler();
            population = populationFactory.Create(
                Network, 
                NeuralNetworkTest.Create("0.0 = 1.0\r\n1.0 = 0.0"), // TODO: Remove this shit.
                out bodyFactory);
            AddBestHandler();
            bestBody = null;
            populationInitialized = false;
        } 

        #endregion

        #region Iteration

        private NeuralNetworkTestResult Step(NeuralNetworkTest test)
        {
            Debug.Assert(population != null);
            Debug.Assert(bodyFactory != null);
            bodyFactory.Parameters.Test = test;
            if (populationInitialized)
            {
                population.Step();
            }
            else
            {
                population.Initialize();
                populationInitialized = true;
            }
            Debug.Assert(bestBody != null);
            return bestBody.TestResult;
        }

        #endregion

        #region Events

        private void AddBestHandler()
        {
            this.population.BestBodyArrived += OnBestBodyArrived;
        }

        private void RemoveBestHandler()
        {
            this.population.BestBodyArrived += OnBestBodyArrived;
        }

        void OnBestBodyArrived(object sender, BestBodyArrivedToGroupEventArgs<DNASequence<double>, AdjustedNetworkBody<NeuralNetwork>> e)
        {
            lock (SyncRoot)
            {
                var body = e.Body as AdjustedNeuralNetworkBody;
                Debug.Assert(body != null);
                if (bestBody == null || body.Error < bestBody.Error)
                {
                    bestBody = body;
                }
            }
        } 

        #endregion

        #region IParallelLearningStrategy Members

        NeuralNetworkTestResult IParallelLearningStrategy.DoIteration(NeuralNetworkTest test)
        {
            return Step(test);
        }

        #endregion
    }
}
