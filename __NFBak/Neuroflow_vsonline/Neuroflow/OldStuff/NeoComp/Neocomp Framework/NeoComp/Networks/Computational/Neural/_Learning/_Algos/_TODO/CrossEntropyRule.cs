using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    public enum DistributionType { Uniform, Gaussian }
    
    public sealed class CrossEntropyRule : LearningRule
    {
        private DistributionType distributionType = DistributionType.Gaussian;

        public DistributionType DistributionType
        {
            get { return distributionType; }
            set { distributionType = value; }
        }
        
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

        private Probability mutationChance = 0.001;

        public Probability MutationChance
        {
            get { return mutationChance; }
            set { mutationChance = value; }
        }

        private double mutationStrength = 0.1;

        public double MutationStrength
        {
            get { return mutationStrength; }
            set 
            {
                Contract.Requires(value >= 0.0 && value <= 1.0);

                mutationStrength = value; 
            }
        }
        
        protected override Type AlgorithmType
        {
            get { return typeof(CrossEntropyAlgorithm); }
        }
    }
}
