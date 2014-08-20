using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Features;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational.Neural
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "neuralComp")]
    [Serializable]
    [KnownType(typeof(NeuralNetwork))]
    public class NeuralComputation : FeaturedComputation
    {
        #region Constructors

        public NeuralComputation(Learning learning, object featuredObject = null)
            : this(learning.Network, learning.NumberOfIterations, featuredObject)
        {
            Contract.Requires(learning != null);
        }

        public NeuralComputation(NeuralNetwork network, int numberOfIterations = 1, object featuredObject = null)
            : base(network, numberOfIterations, featuredObject)
        {
            Contract.Requires(network != null);
            Contract.Requires(numberOfIterations >= 1);
        }  

        #endregion

        #region Clone

        new public NeuralComputation Clone()
        {
            return (NeuralComputation)base.Clone();
        }

        #endregion
    }
}
