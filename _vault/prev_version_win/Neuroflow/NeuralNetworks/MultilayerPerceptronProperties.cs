using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public interface IMultilayerPerceptronProperties
    {
        GradientComputationMethod GradientComputationMethod { get; }
    }

    public sealed class MultilayerPerceptronProperties : IMultilayerPerceptronProperties
    {
        public GradientComputationMethod GradientComputationMethod { get; set; }
    }
}
