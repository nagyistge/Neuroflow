using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class NoisedWeightInitializationRule : LearningRule
    {
        const double DefNoise = 0.3;

        public NoisedWeightInitializationRule()
        {
            Noise = DefNoise;
        }
        
        [Category(PropertyCategories.Math)]
        [InitValue(DefNoise)]
        [DefaultValue(DefNoise)]
        public double Noise { get; set; }

        protected override Type AlgorithmType
        {
            get { return typeof(NoisedWeightInitializationAlgorithm); }
        }
    }
}
