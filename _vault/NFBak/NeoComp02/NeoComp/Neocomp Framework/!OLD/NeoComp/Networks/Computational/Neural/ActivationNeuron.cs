using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;
using System.Threading;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "activNeuron")]
    public class ActivationNeuron : OperationNode<double>, IBackwardConnection, ILearningConnection, IBackwardPropagator
    {
        #region Constructors

        public ActivationNeuron(IActivationFunction activationFunction, params ILearningRule[] learningRules)
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
        public double Bias { get; set; }

        [DataMember(Name = "aFunc")]
        public IActivationFunction ActivationFunction { get; private set; }

        [NonSerialized]
        ReadOnlyArray<ILearningRule> learningRules;

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

        protected sealed override double GenerateOutput(ConnectionEntry<ComputationalConnection<double>>[] inputConnectionEntries)
        {
            return ActivationFunction.Function(Sum(inputConnectionEntries));
        }

        private double Sum(ConnectionEntry<ComputationalConnection<double>>[] inputConnectionEntries)
        {
            double sum = Bias;
            foreach (var entry in inputConnectionEntries) sum += entry.Connection.OutputValue;
            return sum;
        }

        #endregion

        #region BackPropagate

        void IBackwardPropagator.BackPropagate(BackwardConnectionEntry[] outputs, BackwardConnectionEntry[] inputs, bool isNewBatch)
        {
            double error = 0.0;
            foreach (var output in outputs)
            {
                error += output.LastWeightedError;
            }
            error *= ActivationFunction.Derivate(OutputValue.Value);

            backwardValues.AddNext(error, 1.0, isNewBatch);
            foreach (var input in inputs)
            {
                input.AddNextError(error, isNewBatch);
            }
        }

        #endregion
    }
}
