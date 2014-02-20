using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public class NeuralNetworkFactory : ComputationalNetworkFactory<double>
    {
        #region Constructors

        public NeuralNetworkFactory(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
        }

        public NeuralNetworkFactory(NeuralNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);
        } 

        #endregion
    }
}
