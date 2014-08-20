using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public class SigmoidActivationFunction : DerivatableActivationFunction
    {
        #region Create

        public static SigmoidActivationFunction Create(bool bipolar, double alpha = 1.0)
        {
            return bipolar ?
                new BipolarSigmoidActivationFunction(alpha) :
                new SigmoidActivationFunction(alpha);
        }

        #endregion

        #region Constructors

        public SigmoidActivationFunction()
        {
        }

        public SigmoidActivationFunction(double alpha)
            : base(alpha)
        {
        }

        #endregion

        #region Calculate

        public override double Function(double value)
        {
            return (1.0 / (1.0 + Math.Exp(-Alpha * value)));
        }

        #endregion

        #region IDerivatableActivationFunction Members

        public override double Derivate(double value)
        {
            return (Alpha * value * (1.0 - value));
        }

        #endregion
    }

    public sealed class BipolarSigmoidActivationFunction : SigmoidActivationFunction
    {
        #region Constructors

        public BipolarSigmoidActivationFunction()
        {
        }

        public BipolarSigmoidActivationFunction(double alpha)
            : base(alpha)
        {
        }

        #endregion

        #region Calculate

        public override double Function(double value)
        {
            return ((2.0 / (1.0 + Math.Exp(-Alpha * value))) - 1.0);
        }

        #endregion

        #region IDerivatableActivationFunction Members

        public override double Derivate(double value)
        {
            return (Alpha * (1.0 - value * value) / 2.0);
        }

        #endregion
    }
}
