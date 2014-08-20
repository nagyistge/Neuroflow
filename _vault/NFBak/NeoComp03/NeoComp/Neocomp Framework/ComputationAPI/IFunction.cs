using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;

namespace ComputationAPI
{
    [ContractClass(typeof(IFunctionContract))]
    public interface IFunction
    {
        Expression Function(Expression value);

        Expression Derivate(Expression value);

        double Function(double value);

        double Derivate(double value);
    }

    [ContractClassFor(typeof(IFunction))]
    class IFunctionContract : IFunction
    {
        #region IFunction Members

        Expression IFunction.Function(Expression value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<Expression>() != null);
            return null;
        }

        Expression IFunction.Derivate(Expression value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<Expression>() != null);
            return null;
        }

        double IFunction.Function(double value)
        {
            return 0;
        }

        double IFunction.Derivate(double value)
        {
            return 0;
        }

        #endregion
    }
}
