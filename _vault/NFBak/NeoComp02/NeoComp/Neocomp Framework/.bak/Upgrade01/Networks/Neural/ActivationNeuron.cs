using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Adjustables;

namespace NeoComp.Networks.Neural
{
    public class ActivationNeuron : Neuron, IActivationNeuron
    {
        #region Constructors

        public ActivationNeuron(IActivationFunction activationFunction)
        {
            Contract.Requires(activationFunction != null);

            ActivationFunction = activationFunction;
        }

        public ActivationNeuron(double bias, IActivationFunction activationFunction)
        {
            Contract.Requires(activationFunction != null);

            ActivationFunction = activationFunction;
            Bias = bias;
        }

        #endregion

        #region Properties

        public double Bias { get; set; }

        public IActivationFunction ActivationFunction { get; private set; }

        #endregion

        #region Transfer

        protected sealed override bool GenerateOutput(Synapse[] inputConnections, Synapse[] outputConnections, out double output)
        {
            output = Activation(Sum(inputConnections));
            return true;
        }

        protected virtual double Sum(Synapse[] synapses)
        {
            double sum = Bias;
            foreach (var syn in synapses) sum += syn.Output;
            return sum;
        }

        protected virtual double Activation(double value)
        {
            return ActivationFunction.Function(value);
        }

        #endregion

        #region IAdjustable Members

        double IAdjustableItem.Adjustment
        {
            get { return Bias; }
            set { Bias = value; }
        }

        #endregion
    }
}
