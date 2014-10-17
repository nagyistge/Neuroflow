using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Optimization.Learning
{
    public sealed class GARule : BackwardRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(GAAlgorithm); }
        }
        
        private LearningMode mode = LearningMode.Batch;

        public LearningMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }
        
        private int populationSize = 500;

        public int PopulationSize
        {
            get { return populationSize; }
            set { populationSize = value; }
        }

        private double weightValueRange = 1.1;

        public double WeightValueRange
        {
            get { return weightValueRange; }
            set { weightValueRange = value; }
        }

        private IntRange parentCountRange = IntRange.CreateFixed(4);

        public IntRange ParentCountRange
        {
            get { return parentCountRange; }
            set { parentCountRange = value; }
        }

        
        private Probability mutationChance = 0.02;

        public Probability MutationChance
        {
            get { return mutationChance; }
            set { mutationChance = value; }
        }

        private double mutationStrength = 0.1;

        public double MutationStrength
        {
            get { return mutationStrength; }
            set { mutationStrength = value; }
        }

        private int crossoverPoints = 4;

        public int CrossoverPoints
        {
            get { return crossoverPoints; }
            set { crossoverPoints = value; }
        }

        private double bestSelectStdDev = 0.3;

        public double BestSelectStdDev
        {
            get { return bestSelectStdDev; }
            set { bestSelectStdDev = value; }
        }

        private double worstSelectStdDev = 0.01;

        public double WorstSelectStdDev
        {
            get { return worstSelectStdDev; }
            set { worstSelectStdDev = value; }
        }
    }
}
