using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Runtime.Serialization;
using System.Threading;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.Collections;
using Neuroflow.Core.Networks;
using Neuroflow.Core.NeuralNetworks.Learning;
using Neuroflow.Core.NeuralNetworks.ActivationFunctions;
using System.ComponentModel;
using Neuroflow.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Neuroflow.Core.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "activNeuron")]
    public class ActivationNeuron : OperationNode<double>, IBackwardConnection, ILearningConnection, IBackwardNode
    {
        #region Constructors

        public ActivationNeuron(
            [Required]
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("ActivationFunction")]
            IActivationFunction activationFunction,
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("LearningRules")]
            params ILearningRule[] learningRules)
        {
            Contract.Requires(activationFunction != null);

            ActivationFunction = activationFunction;
            if (learningRules != null)
            {
                var fa = learningRules.ToArray();
                if (fa.Length != 0) this.learningRules = ReadOnlyArray.Wrap(fa);
            }
        }

        #endregion

        #region Properties

        [DataMember(Name = "bias")]
        [Browsable(false)]
        public double Bias { get; set; }

        [DataMember(Name = "aFunc")]
        public IActivationFunction ActivationFunction { get; private set; }

        [NonSerialized]
        ReadOnlyArray<ILearningRule> learningRules;

        [Browsable(false)]
        public ReadOnlyArray<ILearningRule> LearningRules
        {
            get { return learningRules; }
        }

        [NonSerialized]
        BackwardValues backwardValues = new BackwardValues();

        BackwardValues IBackwardConnection.BackwardValues
        {
            get { return backwardValues; }
        }

        IEnumerable<ILearningRule> ILearningConnection.LearningRules
        {
            get { return LearningRules; }
        }

        double INeuralConnection.InputValue
        {
            get { return 1.0; }
        }

        double INeuralConnection.OutputValue
        {
            get { return OutputValue.Value; }
        }

        double INeuralConnection.Weight
        {
            get { return Bias; }
            set { Bias = value; }
        }

        #endregion

        #region Transfer

        protected sealed override double GenerateOutput(ConnectionEntry<ComputationConnection<double>>[] inputConnectionEntries)
        {
            return ActivationFunction.Function(Sum(inputConnectionEntries));
        }

        private double Sum(ConnectionEntry<ComputationConnection<double>>[] inputConnectionEntries)
        {
            double sum = Bias;
            foreach (var entry in inputConnectionEntries) sum += entry.Connection.OutputValue;
            return sum;
        }

        #endregion

        #region Backprop

        public Backpropagator CreateBackprogatator()
        {
            return new ActivationNeuronBackpropagator(this);
        } 

        #endregion
    }
}
