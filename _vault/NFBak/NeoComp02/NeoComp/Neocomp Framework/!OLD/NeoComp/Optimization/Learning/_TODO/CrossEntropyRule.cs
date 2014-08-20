using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class CrossEntropyRule : BackwardRule
    {
        private int populationSize = 100;

        public int PopulationSize
        {
            get { return populationSize; }
            set 
            {
                Contract.Requires(value > 0);

                populationSize = value; 
            }
        }

        private int numberOfElites = 10;

        public int NumberOfElites
        {
            get { return numberOfElites; }
            set
            {
                Contract.Requires(value > 0 && value < PopulationSize);

                numberOfElites = value; 
            }
        }

        protected override Type AlgorithmType
        {
            get { return typeof(CrossEntropyAlgorithm); }
        }
    }
}
