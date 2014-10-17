using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class CrossEntropyAlgorithm : GlobalOptimizationLearningAlgorithm
    {
        #region Const

        const double Epsilon = 0.000001;

        const double MaxStdDev = 1.0;

        static readonly DoubleRange MeanRange = new DoubleRange(-1.0, 1.0);

        #endregion

        #region Structures

        struct StatisticalInfo
        {
            internal StatisticalInfo(double mean, double stdDev)
            {
                this.mean = mean;
                this.stdDev = stdDev;
            }

            internal static StatisticalInfo Create()
            {
                var inf = new StatisticalInfo();
                inf.mean = 0.0;
                inf.stdDev = CrossEntropyAlgorithm.MaxStdDev;
                return inf;
            }
            
            private double mean;

            internal double Mean
            {
                get { return mean; }
            }

            private double stdDev;

            internal double StdDev
            {
                get { return stdDev; }
            }

            public override string ToString()
            {
                return string.Format("Mean: {0} StdDev: {1}", mean, stdDev);
            }
        }

        #endregion

        #region Fields

        StatisticalInfo[] infos;

        Population evaluatedWeights;

        Action nextStep;

        CrossEntropyRule rule;

        #endregion

        #region Init

        protected internal override void InitializeNewRun(AlgoInitializationMode mode)
        {
            base.InitializeNewRun(mode);

            if (mode == AlgoInitializationMode.Startup)
            {
                rule = (CrossEntropyRule)LearningConnections[0].Rule;
                int connCount = LearningConnections.Count;
                infos = Enumerable.Range(0, connCount).Select(idx => StatisticalInfo.Create()).ToArray();
                evaluatedWeights = new Population(rule.PopulationSize);
                nextStep = null;
            }
        }

        #endregion

        #region Iteration

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            if (batch)
            {
                if (nextStep == null)
                {
                    BeginCrossEntropy();
                }
                else
                {
                    nextStep();
                }
            }
        }

        #endregion

        #region Cross Entropy

        private void BeginCrossEntropy()
        {
            Action beginEvaluate = null, evaluated = null;

            // Evaluate solutions:
            beginEvaluate = () =>
            {
                int soFar = evaluatedWeights.Count;
                if (soFar != rule.PopulationSize)
                {
                    GenerateWeights();
                    
                    // GOTO: evaluated.
                    nextStep = evaluated;
                }
                else
                {
                    // Evaluation done.
                    UpdateInfos();
                    evaluatedWeights.Clear();

                    // Restart:
                    beginEvaluate();
                }
            };

            // Weights evaluated:
            evaluated = () =>
            {
                Debug.Assert(evaluatedWeights.Count < rule.PopulationSize);

                evaluatedWeights.Add(new SolutionKey(CurrentMSE), GetWeights());

                // GOTO: Begin
                beginEvaluate();
            };

            // Begin Evaluation
            beginEvaluate();
        }

        private void UpdateInfos()
        {
            for (int idx = 0; idx < infos.Length; idx++)
            {
                if (rule.DistributionType == DistributionType.Gaussian)
                {
                    double mean, stdDev;
                    Statistics.CalculateMeanAndStdDev(GetEvaluatedEliteWeights(idx), out mean, out stdDev);

                    //if (stdDev < rule.MutationStrength && rule.MutationChance)
                    //{
                    //    stdDev += rule.MutationStrength;
                    //}

                    // Range checking:
                    mean = MeanRange.Cut(mean);
                    if (double.IsNaN(stdDev) || stdDev < Epsilon)
                    {
                        stdDev = Epsilon;
                    }
                    else if (stdDev > MaxStdDev)
                    {
                        stdDev = MaxStdDev;
                    }

                    infos[idx] = new StatisticalInfo(mean, stdDev);
                }
                else
                {
                    double min, max;
                    CalculateMinMax(GetEvaluatedEliteWeights(idx), out min, out max);
                    double d = (max - min) / 2.0;
                    if (d < rule.MutationStrength && rule.MutationChance)
                    {
                        double d2 = (d + rule.MutationStrength) / 2.0;
                        min -= d2;
                        max += d2;
                        d = (max - min) / 2.0;
                    }
                    min = MeanRange.Cut(min);
                    max = MeanRange.Cut(max);

                    infos[idx] = new StatisticalInfo(min + d, d);
                }
            }
        }

        private void CalculateMinMax(IEnumerable<double> numbers, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            foreach (var num in numbers)
            {
                if (num < min) min = num; else if (num > max) max = num;
            }
        }

        IEnumerable<double> GetEvaluatedEliteWeights(int weightIndex)
        {
            int ne = rule.NumberOfElites;
            for (int idx = 0; idx < ne; idx++)
            {
                double weight = evaluatedWeights.Values[idx][weightIndex];
                yield return weight;
            }
        }

        void GenerateWeights()
        {
            var a = LearningConnections.ItemArray;
            for (int idx = 0; idx < infos.Length; idx++)
            {
                var info = infos[idx];
                if (rule.DistributionType == DistributionType.Gaussian)
                {
                    a[idx].Connection.Weight = Statistics.GenerateGauss(info.Mean, info.StdDev);

                    if (rule.MutationChance)
                    {
                        double w = a[idx].Connection.Weight;
                        w += RandomGenerator.NextDouble(w - rule.MutationStrength, w + rule.MutationStrength);
                        a[idx].Connection.Weight = w;
                    }
                }
                else
                {
                    double d = info.StdDev;
                    double min = info.Mean - d;
                    double max = info.Mean + d;
                    a[idx].Connection.Weight = RandomGenerator.NextDouble(min, max);
                }
            }
        }

        double[] GetWeights()
        {
            var a = LearningConnections.ItemArray;
            double[] weights = new double[a.Length];
            for (int idx = 0; idx < weights.Length; idx++)
            {
                weights[idx] = a[idx].Connection.Weight;
            }
            return weights;
        }

        #endregion
    }
}
