using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "afLin")]
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
            //if (value < -Alpha) return -0.000001;
            //if (value > Alpha) return 0.000001;
            //if (value < -Alpha || value > Alpha) return 0;
            return Alpha;
        }

        #endregion
    }
}
