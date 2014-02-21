using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Networks.Neural;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class BackPropagationLearningStrategy : 
        SpecializedLearningStrategy<IBackPropagationSynapse, IBackPropagationNeuron>,
        IOnlineLearningStrategy
    {
        #region Constructor

        public BackPropagationLearningStrategy(NeuralNetwork network, double learningRate = 0.1, double momentum = 0.0)
            : base(network)
        {
            Contract.Requires(network != null);
            Contract.Requires(learningRate > 0.0 && learningRate <= 1.0);
            Contract.Requires(momentum >= 0.0 && momentum <= 1.0);

            this.learningRate = learningRate;
            this.momentum = momentum;
        } 

        #endregion

        #region Fields

        IDerivatableActivationFunction lastAC;

        ConnectionInfo<IBackPropagationSynapse> outputConnections;

        #endregion

        #region Properties

        double learningRate;

        public double LearningRate
        {
            get { lock (SyncRoot) return learningRate; }
            set
            {
                Contract.Requires(value > 0.0 && value <= 1.0);

                lock (SyncRoot) learningRate = value;
            }
        }

        double momentum;

        public double Momentum
        {
            get { lock (SyncRoot) return momentum; }
            set
            {
                Contract.Requires(value >= 0.0 && value <= 1.0);

                lock (SyncRoot) momentum = value;
            }
        }

        private ConnectedNode<IBackPropagationSynapse, IBackPropagationNeuron>[] ConnectedNeuronArray
        {
            get { return ConnectedNeurons.ConnectedNodeArray; }
        }

        #endregion

        #region Implementation

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            lock (Network.SyncRoot)
            {
                lock (SyncRoot)
                {
                    if (outputConnections == null) outputConnections = Network.GetOutputConnectionInfo<IBackPropagationSynapse>();
                    
                    lastAC = ConnectedNeuronArray[ConnectedNeuronArray.Length - 1].Node.ActivationFunction;

                    foreach (var cn in ConnectedNeuronArray)
                    {
                        foreach (var synapse in cn.UpperConnectionArray)
                        {
                            synapse.Weight = RandomGenerator.NextDouble(-1.0, 1.0);
                            synapse.Error = 0.0;
                            synapse.Delta = 0.0;
                        }
                        var neuron = cn.Node;
                        neuron.Bias = RandomGenerator.NextDouble(-1.0, 1.0);
                        neuron.Error = 0.0;
                        neuron.Delta = 0.0;
                    }

                    foreach (var info in outputConnections)
                    {
                        foreach (var synapse in info)
                        {
                            synapse.Weight = 1.0;
                            synapse.Error = 0.0;
                            synapse.Delta = 0.0;
                        }
                    }
                }
            }
        }

        private void BackPropagate(IEnumerable<double> errors)
        {
            Contract.Requires(errors != null);
            Contract.Requires(errors.Count() == Network.OutputInterface.Length);
            
            lock (Network.SyncRoot)
            {
                lock (SyncRoot)
                {
                    int index = 0;
                    foreach (double error in errors)
                    {
                        if (index >= Network.OutputInterface.Length)
                        {
                            throw GetInvalidNumberOfErrorsEx();
                        }

                        SetErrorsToOutputConnections(index, error);

                        index++;
                    }
                    if (index != Network.OutputInterface.Length)
                    {
                        throw GetInvalidNumberOfErrorsEx();
                    }

                    BackPropagateErrors();
                    UpdateAdjustments();
                }
            }
        }

        private void SetErrorsToOutputConnections(int index, double error)
        {
            foreach (var syn in outputConnections[index])
            {
                syn.Error = error;
            }
        }

        private void BackPropagateErrors()
        {
            for (int idx = ConnectedNeuronArray.Length - 1; idx >= 0; idx--)
            {
                var cn = ConnectedNeuronArray[idx];
                BackPropagateErrors(cn);
            }
        }

        private void BackPropagateErrors(ConnectedNode<IBackPropagationSynapse, IBackPropagationNeuron> connectedNeuron)
        {
            var neuron = connectedNeuron.Node;
            var lowerSynapses = connectedNeuron.LowerConnectionArray;
            var upperSynapses = connectedNeuron.UpperConnectionArray;

            neuron.Error = 0.0;
            foreach (var lSynapse in lowerSynapses)
            {
                neuron.Error += lSynapse.Weight * lSynapse.Error;
            }

            neuron.Error = neuron.ActivationFunction.Derivate(neuron.Output) * neuron.Error;
            
            foreach (var uSynapse in upperSynapses)
            {
                uSynapse.Error = neuron.Error;
            }
        }

        private void UpdateAdjustments()
        {
            foreach (var cn in ConnectedNeuronArray)
            {
                UpdateAdjustments(cn);
            }
        }

        private void UpdateAdjustments(ConnectedNode<IBackPropagationSynapse, IBackPropagationNeuron> connectedNeuron)
        {
            var neuron = connectedNeuron.Node;
            var upperSynapses = connectedNeuron.UpperConnectionArray; 
            
            foreach (var uSynapse in upperSynapses)
            {
                Adjust(uSynapse, learningRate * uSynapse.Error * uSynapse.Input);
            }
            Adjust(neuron, learningRate * neuron.Error);
        }

        private void Adjust(IDeltaBasedAdjustable adjustableItem, double delta)
        {
            if (momentum == 0.0)
            {
                adjustableItem.Adjustment += (adjustableItem.Delta = delta);
            }
            else
            {
                double value = momentum * adjustableItem.Delta;
                value += (1.0 - momentum) * delta;
                adjustableItem.Adjustment += (adjustableItem.Delta = value);
            }
        }

        private static InvalidOperationException GetInvalidNumberOfErrorsEx()
        {
            return new InvalidOperationException("Invalid number of errors.");
        }

        #endregion

        #region IOnlineAdjusterAlgorithm Members

        void IOnlineLearningStrategy.Adjust(double mse, IEnumerable<double> errors, int sampleCount)
        {
            BackPropagate(errors);
        }

        #endregion
    }
}
