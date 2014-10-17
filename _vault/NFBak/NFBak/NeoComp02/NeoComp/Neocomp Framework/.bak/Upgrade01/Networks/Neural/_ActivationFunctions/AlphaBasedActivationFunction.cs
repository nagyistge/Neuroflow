using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public abstract class AlphaBasedActivationFunction : ActivationFunction
    {
        #region Constructors

        protected AlphaBasedActivationFunction()
        {
        }

        protected AlphaBasedActivationFunction(double alpha)
        {
            Alpha = alpha;
        }

        #endregion

        #region Properties

        public double Alpha { get; set; }

        #endregion
    }
}
