using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public sealed class WeightDecayRule : LearningRule
    {
        public WeightDecayRule()
        {
            Cutoff = 2.0;
        }

        double cutoff;

        public double Cutoff
        {
            get { return cutoff; }
            set
            {
                Contract.Requires(value >= 0.0);

                cutoff = value;
                Cutoff4 = Math.Pow(value, 4.0);
            }
        }

        internal double Cutoff4 { get; private set; }

        double factor = -0.0001;

        public double Factor
        {
            get { return factor; }
            set
            {
                Contract.Requires(value <= 0.0);

                factor = value;
            }
        }
        
        protected override Type AlgorithmType
        {
            get { return typeof(WeightDecayAlgorithm); }
        }
    }
}
