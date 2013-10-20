using Neuroflow.Data;
using Neuroflow.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public abstract class ComputationContext : DisposableObject
    {
        public abstract Device Device { get; }

        public virtual DataArrayFactory DataArrayFactory
        {
            get { throw GetNSEx("Data Array Factory"); }
        }

        NeuralNetworkFactory neuralNetworkFactory;

        public NeuralNetworkFactory NeuralNetworkFactory
        {
            get { return neuralNetworkFactory ?? (neuralNetworkFactory = new NeuralNetworkFactory(MultilayerPerceptronAdapter)); }
        }

        public virtual VectorUtils VectorUtils
        {
            get { throw GetNSEx("Vector Utils"); }
        }

        protected virtual IMultilayerPerceptronAdapter MultilayerPerceptronAdapter
        {
            get { throw GetNSEx("Multilayer Perceptron Adapter"); }
        }

        private static NotSupportedException GetNSEx(string name)
        {
            return new NotSupportedException(name + " feature is not supported.");
        }
    }
}
