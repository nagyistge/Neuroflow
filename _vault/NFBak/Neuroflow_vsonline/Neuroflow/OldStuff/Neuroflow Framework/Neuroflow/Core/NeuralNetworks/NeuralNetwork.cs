using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Computations;
using Neuroflow.Core;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.NeuralNetworks.ActivationFunctions;

namespace Neuroflow.Core.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "neuralNetwork")]
    [KnownType("GetFeaturedTypes")]
    public class NeuralNetwork : ComputationalNetwork<double>
    {
        public NeuralNetwork(NeuralNetworkFactory factory)
            : base(factory)
        {
            Contract.Requires(factory != null);
        }

        private static Type[] GetFeaturedTypes()
        {
            return new[] 
            { 
                typeof(Synapse), 
                typeof(ActivationNeuron), 
                typeof(NeuralConnection), 
                typeof(SigmoidActivationFunction), 
                typeof(LinearActivationFunction),
                typeof(ComputationalNetworkInterface<double>)
            };
        }

        #region Clone

        new public NeuralNetwork Clone()
        {
            return (NeuralNetwork)base.Clone();
        }

        #endregion
    }
}
