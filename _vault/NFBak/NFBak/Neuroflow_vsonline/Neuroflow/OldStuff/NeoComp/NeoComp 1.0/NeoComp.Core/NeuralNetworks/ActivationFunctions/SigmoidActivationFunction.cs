using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NeoComp.NeuralNetworks.ActivationFunctions
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "afSig")]
    public sealed class SigmoidActivationFunction : IActivationFunction
    {
        #region Constructors

        public SigmoidActivationFunction()
        {
        }

        public SigmoidActivationFunction(double alpha)
        {
            Alpha = alpha;
        }

        #endregion

        #region Properties

        [DataMember(Name = "a")]
        public double Alpha { get; set; }

        #endregion

        #region Calculate

        public double Function(double value)
        {
            return ((2.0 / (1.0 + Math.Exp(-Alpha * value))) - 1.0);
        }

        public double Derivate(double value)
        {
            return (Alpha * (1.0 - value * value) / 2.0);
        }

        #endregion
    }
}
