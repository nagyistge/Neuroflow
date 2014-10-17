using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public class LinearActivationFunction : AlphaBasedActivationFunction
    {
        #region Create

        public static LinearActivationFunction Create(bool bipolar, double alpha = 1.0)
        {
            return bipolar ?
                new BipolarLinearActivationFunction(alpha) :
                new LinearActivationFunction(alpha);
        }

        #endregion

        #region Constructors

        public LinearActivationFunction()
        {
        }

        public LinearActivationFunction(double alpha)
            : base(alpha)
        {
        }

        #endregion

        #region Calculate

        public override double Function(double value)
        {
            double result = (value * Alpha) + 0.5;
            if (result < 0.0) return 0.0; else if (result > 1.0) return 1.0;
            return result;
        }

        #endregion
    }

    public sealed class BipolarLinearActivationFunction : LinearActivationFunction
    {
        #region Constructors

        public BipolarLinearActivationFunction()
        {
        }

        public BipolarLinearActivationFunction(double alpha)
            : base(alpha)
        {
        }

        #endregion

        #region Calculate

        public override double Function(double value)
        {
            double result = (value * Alpha);
            if (result < -1.0) return -1.0; else if (result > 1.0) return 1.0;
            return result;
        }

        #endregion
    }
}
