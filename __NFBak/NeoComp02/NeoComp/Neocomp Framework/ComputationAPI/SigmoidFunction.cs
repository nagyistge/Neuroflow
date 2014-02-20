using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ComputationAPI
{
    public sealed class SigmoidFunction : IFunction
    {
        #region Constructors

        public SigmoidFunction()
        {
            Alpha = 1.0;
        }

        public SigmoidFunction(double alpha)
        {
            Alpha = alpha;
        }

        #endregion

        #region Properties

        public double Alpha { get; set; }

        #endregion
        
        #region IFunction Members

        public Expression Function(Expression value)
        {
            var oneEx = Expression.Constant(1.0);
            var twoEx = Expression.Constant(2.0);
            var aEx = Expression.Constant(-Alpha);

            // -Alpha * value:
            Expression ex = Expression.Multiply(aEx, value);

            //  Math.Exp(-Alpha * value):
            ex = Expression.Call(typeof(Math).GetMethod("Exp"), ex);

            // (1.0 + Math.Exp(-Alpha * value))
            ex = Expression.Add(oneEx, ex);

            // (2.0 / (1.0 + Math.Exp(-Alpha * value)))
            ex = Expression.Divide(twoEx, ex);

            // ((2.0 / (1.0 + Math.Exp(-Alpha * value))) - 1.0)
            ex = Expression.Subtract(ex, oneEx);

            return ex;
        }

        public Expression Derivate(Expression value)
        {
            var oneEx = Expression.Constant(1.0);
            var twoEx = Expression.Constant(2.0);
            var aEx = Expression.Constant(-Alpha);

            // value * value:
            Expression ex = Expression.Multiply(value, value);

            // 1.0 - value * value:
            ex = Expression.Subtract(oneEx, ex);

            // Alpha * (1.0 - value * value):
            ex = Expression.Multiply(aEx, ex);

            // (Alpha * (1.0 - value * value) / 2.0)
            ex = Expression.Multiply(ex, twoEx);

            return ex;
        }

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
