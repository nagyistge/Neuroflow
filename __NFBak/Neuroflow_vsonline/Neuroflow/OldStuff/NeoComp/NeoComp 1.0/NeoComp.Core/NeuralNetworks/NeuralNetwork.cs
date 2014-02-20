using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using NeoComp.ComputationalNetworks;
using NeoComp.NeuralNetworks.ActivationFunctions;

namespace NeoComp.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "neuralNetwork")]
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
