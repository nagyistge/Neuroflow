using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using Neuroflow.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.ActivationFunctions
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "afSig")]
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
        [InitValue(1.0)]
        [DefaultValue(1.0)]
        [Category(PropertyCategories.Math)]
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
