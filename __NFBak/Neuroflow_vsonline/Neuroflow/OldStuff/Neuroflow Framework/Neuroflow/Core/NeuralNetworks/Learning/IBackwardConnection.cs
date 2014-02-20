using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    [ContractClass(typeof(IBackwardConnectionContract))]
    public interface IBackwardConnection : INeuralConnection
    {
        BackwardValues BackwardValues { get; }
    }

    [ContractClassFor(typeof(IBackwardConnection))]
    class IBackwardConnectionContract : IBackwardConnection
    {
        BackwardValues IBackwardConnection.BackwardValues
        {
            get
            {
                Contract.Ensures(Contract.Result<BackwardValues>() != null);
                return null;
            }
        }

        double INeuralConnection.InputValue
        {
            get { throw new NotImplementedException(); }
        }

        double INeuralConnection.OutputValue
        {
            get { throw new NotImplementedException(); }
        }

        double INeuralConnection.Weight
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #region IReset Members

        void Computations.IReset.Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
