using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    [ContractClass(typeof(INeuralArchitectureContract))]
    public interface INeuralArchitecture
    {
        NeuralNetworkFactory CreateFactory();

        NeuralNetwork CreateNetwork();
    }

    [ContractClassFor(typeof(INeuralArchitecture))]
    class INeuralArchitectureContract : INeuralArchitecture
    {
        NeuralNetworkFactory INeuralArchitecture.CreateFactory()
        {
            Contract.Ensures(Contract.Result<NeuralNetworkFactory>() != null);
            
            return null;
        }

        NeuralNetwork INeuralArchitecture.CreateNetwork()
        {
            Contract.Ensures(Contract.Result<NeuralNetwork>() != null); 
            
            return null;
        }
    }
}
