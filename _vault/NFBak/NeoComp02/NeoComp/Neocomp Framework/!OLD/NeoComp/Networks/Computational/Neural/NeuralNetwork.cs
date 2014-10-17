using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "neuralNetwork")]
    [KnownType("GetFeaturedTypes")]
    public class NeuralNetwork : ValueComputationalNetwork<double>
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
                typeof(ValueComputationalNetworkInterface<double>)
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
