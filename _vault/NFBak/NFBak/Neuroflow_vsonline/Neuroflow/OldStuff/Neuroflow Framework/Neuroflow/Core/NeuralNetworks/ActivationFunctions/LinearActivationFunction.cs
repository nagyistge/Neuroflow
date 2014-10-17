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
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "afLin")]
    public sealed class LinearActivationFunction : IActivationFunction
    {
        #region Constructors

        public LinearActivationFunction()
        {
            Alpha = 1.0;
        }

        public LinearActivationFunction(double alpha)
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
            double result = (value * Alpha);
            if (result < -Alpha) return -Alpha; else if (result > Alpha) return Alpha;
            return result;
        }

        public double Derivate(double value)
        {
            return Alpha;
        }

        #endregion
    }
}
