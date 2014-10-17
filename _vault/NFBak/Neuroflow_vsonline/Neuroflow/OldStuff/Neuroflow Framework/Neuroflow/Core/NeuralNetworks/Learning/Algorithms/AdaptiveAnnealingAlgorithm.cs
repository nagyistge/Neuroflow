using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class AdaptiveAnnealingAlgorithm : GlobalOptimizationLearningAlgorithm
    {
        new private AdaptiveAnnealingRule Rule
        {
            get { return (AdaptiveAnnealingRule)base.Rule; }
        }

        private double CurrentDistance
        {
            get { return Math.Sqrt(CurrentMSE) * 2.0; }
        }

        Action next;

        double[] weights;

        double lastMSE;

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            weights = new double[LearningConnections.ItemArray.Length];
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            if (batch)
            {
                if (next != null)
                {
                    next();
                }
                else
                {
                    Begin();
                }
            }
        }

        private void Begin()
        {
            SaveWeights();
            GenerateWeights();
            TestNewWeights(() =>
                {
                    if (CurrentMSE > lastMSE ||
                        (CurrentMSE == lastMSE && RandomGenerator.FiftyPercentChance) ||
                        CanAccept())
                    {
                        RestoreWeights();
                    }
                    Begin();
                });
        }

        private bool CanAccept()
        {
            bool canAccept = RandomGenerator.Random.NextDouble() < (1.0 - CurrentDistance) * Rule.AcceptProbMul;
            return canAccept;
        }

        private void TestNewWeights(Action toDoAfter)
        {
            lastMSE = CurrentMSE;
            next = toDoAfter;
        }

        private void GenerateWeights()
        {
            double d2 = (CurrentDistance * Rule.WeightGenMul) / 2.0;
            for (int idx = 0; idx < weights.Length; idx++)
            {
                double cw = LearningConnections.ItemArray[idx].Connection.Weight;
                cw = RandomGenerator.NextDouble(cw - d2, cw + d2);
                LearningConnections.ItemArray[idx].Connection.Weight = WeightRange.Cut(cw);
            }
        }

        private void SaveWeights()
        {
            for (int idx = 0; idx < weights.Length; idx++) weights[idx] = LearningConnections.ItemArray[idx].Connection.Weight;
        }

        private void RestoreWeights()
        {
            for (int idx = 0; idx < weights.Length; idx++) LearningConnections.ItemArray[idx].Connection.Weight = weights[idx];
        }
    }
}
