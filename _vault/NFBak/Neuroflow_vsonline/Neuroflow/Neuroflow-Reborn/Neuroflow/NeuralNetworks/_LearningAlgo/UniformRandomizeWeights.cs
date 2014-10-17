using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class UniformRandomizeWeights : LayerLearningInitializationBehavior
    {
        public UniformRandomizeWeights() : 
            this(0.3f)
        {
        }

        public UniformRandomizeWeights(float strength)
        {
            Strength = strength;
        }

        public float Strength { get; set; }

        protected override bool PropsEquals(LayerBehavior other)
        {
            return Strength == ((UniformRandomizeWeights)other).Strength;
        }

        protected override int GenerateHashCode()
        {
            return Strength.GetHashCode();
        }
    }
}
