using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.GeneticNetworks
{
    public struct ConfiguredNeuralGeneData
    {
        #region Constructor

        public ConfiguredNeuralGeneData(double weight, double bias)
        {
            this.weight = weight;
            this.bias = bias;
        }

        #endregion

        #region Properies

        double weight;

        public double Weight
        {
            get { return weight; }
        }

        double bias;

        public double Bias
        {
            get { return bias; }
        } 

        #endregion
    }
}
