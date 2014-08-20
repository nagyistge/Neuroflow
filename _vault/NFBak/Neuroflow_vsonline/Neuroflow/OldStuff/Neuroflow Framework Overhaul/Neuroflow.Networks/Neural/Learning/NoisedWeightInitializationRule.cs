﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
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

        public override Type AlgorithmType
        {
            get { return typeof(NoisedWeightInitializationAlgorithm); }
        }
    }
}
