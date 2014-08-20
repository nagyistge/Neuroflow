using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Neural
{
    public class ObservableActivationNeuron : ActivationNeuron, IObservableActivationNeuron
    {
        #region Constructors

        public ObservableActivationNeuron(IActivationFunction activationFunction)
            : base(activationFunction)
        {
            Contract.Requires(activationFunction != null);
        }

        public ObservableActivationNeuron(double bias, IActivationFunction activationFunction)
            : base(bias, activationFunction)
        {
            Contract.Requires(activationFunction != null);
        }

        #endregion

        #region Properties

        public double Input { get; private set; }

        public double Output { get; private set; }

        #endregion

        #region Overrides

        protected override double Sum(Synapse[] synapses)
        {
            return Input = base.Sum(synapses);
        }

        protected override double Activation(double value)
        {
            return Output = base.Activation(value);
        }

        #endregion
    }
}
