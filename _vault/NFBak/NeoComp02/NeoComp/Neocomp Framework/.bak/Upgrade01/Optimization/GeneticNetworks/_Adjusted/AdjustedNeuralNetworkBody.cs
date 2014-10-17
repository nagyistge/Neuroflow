using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.GA;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedNeuralNetworkBody : AdjustedTestableNetworkBody<NeuralNetwork>
    {
        #region Constructors

        public AdjustedNeuralNetworkBody(DNASequence<double> dna, AdjustedTestableNetworkParameters parameters)
            : base(dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        public AdjustedNeuralNetworkBody(Guid uid, DNASequence<double> dna, AdjustedTestableNetworkParameters parameters)
            : base(uid, dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        new public NeuralNetworkTestResult TestResult
        {
            get { return (NeuralNetworkTestResult)base.TestResult; }
        }

        #endregion

        #region Create Network

        protected override void Setup(NeuralNetwork network)
        {
            Contract.Assert(Parameters.AdjustableItems != null);
            Contract.Assert(Parameters.AdjustableItems.Length == this.Plan.Count);

            var items = Parameters.AdjustableItems;
            for (int idx = 0; idx < items.Length; idx++)
            {
                double adjustment = Plan[idx] * 2.0 - 1.0;
                items[idx].Adjustment = adjustment;
            }


            //int idx = 0;
            //foreach (var synapse in network.GetConnections())
            //{
            //    double adjustment = Plan[idx++] * 2.0 - 1.0;
            //    synapse.Weight = adjustment;
            //}
            //foreach (var neuron in network.GetNodes().Cast<ActivationNeuron>())
            //{
            //    double adjustment = Plan[idx++] * 2.0 - 1.0;
            //    neuron.Bias = adjustment;
            //}
        }

        #endregion
    }
}
