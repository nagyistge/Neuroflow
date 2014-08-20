using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;

namespace NeoComp.Networks.Neural
{
    public class NeuralNetwork : ComputationalNetwork<Synapse, double>
    {
        #region Constructor

        public NeuralNetwork(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
        } 

        #endregion
    }
}
