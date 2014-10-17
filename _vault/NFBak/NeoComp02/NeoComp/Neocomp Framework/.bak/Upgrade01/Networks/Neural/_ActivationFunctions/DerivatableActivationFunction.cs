using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public abstract class DerivatableActivationFunction : AlphaBasedActivationFunction, IDerivatableActivationFunction
    {
        #region Constructors

        protected DerivatableActivationFunction()
            : base()
        {
        }

        protected DerivatableActivationFunction(double alpha)
            : base(alpha)
        {
        }

        #endregion

        #region IDerivatableActivationFunction Members

        public abstract double Derivate(double value);

        #endregion
    }
}
