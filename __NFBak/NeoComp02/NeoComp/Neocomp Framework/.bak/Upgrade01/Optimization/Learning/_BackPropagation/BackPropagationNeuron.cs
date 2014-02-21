using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public class BackPropagationNeuron : ObservableActivationNeuron, IBackPropagationNeuron
    {
        public BackPropagationNeuron(IDerivatableActivationFunction activationFunction)
            : base(activationFunction)
        {
            Contract.Requires(activationFunction != null);
        }

        public BackPropagationNeuron(double bias, IDerivatableActivationFunction activationFunction)
            : base(bias, activationFunction)
        {
            Contract.Requires(activationFunction != null);
        }

        new public IDerivatableActivationFunction ActivationFunction
        {
            get { return (IDerivatableActivationFunction)base.ActivationFunction; }
        }

        public double Delta { get; internal set; }

        public double Error { get; internal set; }

        double IDeltaBasedAdjustable.Delta
        {
            get { return Delta; }
            set { Delta = value; }
        }

        double IBackPropagationNeuron.Error
        {
            get { return Error; }
            set { Error = value; }
        }
    }
}
