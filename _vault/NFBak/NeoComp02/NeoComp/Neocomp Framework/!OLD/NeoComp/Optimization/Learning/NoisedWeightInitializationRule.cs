﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class NoisedWeightInitializationRule : LearningRule
    {
        double noise = 0.3;

        public double Noise
        {
            get { return noise; }
            set
            {
                Contract.Requires(value > 0.0);

                noise = value;
            }
        }

        protected override Type AlgorithmType
        {
            get { return typeof(NoisedWeightInitializationAlgorithm); }
        }
    }
}
