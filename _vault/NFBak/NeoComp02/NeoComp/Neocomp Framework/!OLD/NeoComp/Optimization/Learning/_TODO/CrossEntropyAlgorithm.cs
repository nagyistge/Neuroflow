using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;

namespace NeoComp.Optimization.Learning
{
    public sealed class CrossEntropyAlgorithm : BackwardLearningAlgorithm
    {
        #region Const

        const double Epsilon = 0.00000000001;

        const double MaxStdDev = 0.7;

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

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            rule = (CrossEntropyRule)LearningConnections[0].Rule;
            int connCount = LearningConnections.Count;
            infos = Enumerable.Range(0, connCount).Select(idx => StatisticalInfo.Create()).ToArray();
            evaluatedWeights = new Population(rule.PopulationSize);
            nextStep = null;
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
                double mean, stdDev;
                Statistics.CalculateMeanAndStdDev(GetEvaluatedEliteWeights(idx), out mean, out stdDev);

                if (stdDev < 0.1 && new Probability(0.004))
                {
                    //mean = AddNoise(mean, 0.01);
                    //stdDev = AddNoise(stdDev, 0.01);
                    stdDev += 0.1;
                }

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

        private double AddNoise(double value, double level)
        {
            return value + Statistics.GenerateGauss(0.0, level);
        }

        void GenerateWeights()
        {
            var a = LearningConnections.ItemArray;
            for (int idx = 0; idx < infos.Length; idx++)
            {
                var info = infos[idx];
                a[idx].Connection.Weight = Statistics.GenerateGauss(info.Mean, info.StdDev);
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
