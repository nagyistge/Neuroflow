using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Neural
{
    public class ActivationNeuronBackpropagator : Backpropagator
    {
        internal ActivationNeuronBackpropagator(ActivationNeuron neuron)
            : base(neuron)
        {
            Neuron = neuron;
            biasConn = (IBackwardConnection)neuron;
        }

        public ActivationNeuron Neuron { get; private set; }

        IBackwardConnection biasConn;

        bool recurrent, recurrentStart;

        Stack<double>[] inputs;

        Stack<double> derivates;

        protected internal override void TrackForwardInformation()
        {
            recurrent = true;
            recurrentStart = true;

            if (derivates == null) derivates = new Stack<double>();
            if (inputs == null) inputs = Enumerable.Range(0, InputConnectionEntries.Count).Select(i => new Stack<double>()).ToArray();

            // Track info:
            double deriv = Neuron.ActivationFunction.Derivate(Neuron.OutputValue.Value);
            derivates.Push(deriv);
            for (int idx = 0; idx < inputs.Length; idx++) inputs[idx].Push(InputConnectionEntries.ItemArray[idx].Connection.InputValue);
        }

        protected internal override void Backpropagate()
        {
            double error = GetCurrentError();
            if (recurrent)
            {
                if (recurrentStart)
                {
                    biasConn.BackwardValues.Set(0.0, 0.0);
                    for (int idx = 0; idx < InputConnectionEntries.ItemArray.Length; idx++)
                    {
                        var ice = InputConnectionEntries.ItemArray[idx];
                        ice.Connection.BackwardValues.Set(0.0, 0.0);
                    }
                    recurrentStart = false;
                }

                bool recurrentEnd = derivates.Count == 0;

                if (!recurrentEnd)
                {
                    biasConn.BackwardValues.Set(error, biasConn.BackwardValues.Last.Gradient + error);
                    for (int idx = 0; idx < InputConnectionEntries.ItemArray.Length; idx++)
                    {
                        var ice = InputConnectionEntries.ItemArray[idx];
                        ice.Connection.BackwardValues.Set(error, ice.Connection.BackwardValues.Last.Gradient + error * inputs[idx].Pop());
                    }
                }
                else
                {
                    biasConn.BackwardValues.Add(error, (biasConn.BackwardValues.Last.Gradient + error));
                    for (int idx = 0; idx < InputConnectionEntries.ItemArray.Length; idx++)
                    {
                        var ice = InputConnectionEntries.ItemArray[idx];
                        ice.Connection.BackwardValues.Add(error, (ice.Connection.BackwardValues.Last.Gradient + error * inputs[idx].Pop()));
                    }
                    recurrent = false;
                }
            }
            else
            {
                // Feed forward:
                biasConn.BackwardValues.Add(error, error);
                foreach (var inputConnEntry in InputConnectionEntries.ItemArray)
                {
                    inputConnEntry.Connection.BackwardValues.Add(error, error * inputConnEntry.Connection.InputValue);
                }
            }
        }

        private double GetCurrentError()
        {
            double error = 0.0;
            foreach (var outputConnEntry in OutputConnectionEntries.ItemArray)
            {
                error += outputConnEntry.Connection.Weight * outputConnEntry.Connection.BackwardValues.Last.Error;
            }
            error *= GetCurrentOutputValueDerivate();
            return error;
        }

        private double GetCurrentOutputValueDerivate()
        {
            if (!recurrent) return Neuron.ActivationFunction.Derivate(Neuron.OutputValue.Value);
            try
            {
                return derivates.Pop();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Internal backpropagation logic error. ActivationNeuron's outputValueDerivateStack is empty.");
            }
        }
    }
}
