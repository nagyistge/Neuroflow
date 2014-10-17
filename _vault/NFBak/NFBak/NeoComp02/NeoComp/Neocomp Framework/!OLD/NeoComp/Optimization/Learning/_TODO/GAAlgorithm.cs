using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Optimization.Algorithms.Selection;

namespace NeoComp.Optimization.Learning
{
    public sealed class GAAlgorithm : BackwardLearningAlgorithm
    {
        #region Fields

        Population population;

        GARule rule;

        Action nextStep;

        GaussianSelectionAlgorithm bestSel, worstSel;

        #endregion

        #region Init

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            rule = (GARule)LearningConnections.ItemArray[0].Rule;
            population = new Population(rule.PopulationSize);
            bestSel = new GaussianSelectionAlgorithm(rule.BestSelectStdDev);
            worstSel = new GaussianSelectionAlgorithm(rule.WorstSelectStdDev, SelectionDirection.FromBottom);
            nextStep = null;
        }

        #endregion

        #region Iteration

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            if (rule.Mode == LearningMode.Batch && batch)
            {
                if (nextStep == null) BeginGA(); else nextStep();
                return;
            }

            if (rule.Mode == LearningMode.Stochastic && !batch)
            {
                if (nextStep == null) BeginGA(); else nextStep();
                return;
            }
        }

        #endregion

        #region GA

        private void BeginGA()
        {
            Action fillPopulation = null, life = null;

            fillPopulation = () =>
            {
                if (population.Count < population.Size)
                {
                    RandomizeWeights();

                    nextStep = () =>
                    {
                        population.Add(CurrentMSE, GetWeights());
                        
                        fillPopulation();
                    };
                }
                else
                {
                    // Filled.
                    life();
                }
            };

            life = () =>
            {
                RemoveWorst();
                CreateNewOffspring(); // After this method, weights = new offspring genes.

                nextStep = () =>
                {
                    population.Add(CurrentMSE, GetWeights());

                    life();
                };
            };         

            fillPopulation();
        }

        private void CreateNewOffspring()
        {
            var a = LearningConnections.ItemArray;
            bool start = true;
            foreach (var parent in SelectParents().Reverse())
            {
                if (start)
                {
                    for (int idx = 0; idx < parent.Length; idx++) a[idx].Connection.Weight = parent[idx];
                    start = false;
                }
                else
                {
                    var coPoints = CreateCrossoverPoints();
                    bool up = true;
                    for (int idx = 0; idx < parent.Length; idx++)
                    {
                        // Flip:
                        if (coPoints.Contains(idx))
                        {
                            up = !up;
                        }

                        if (!up) a[idx].Connection.Weight = parent[idx];
                    }
                }
            }
            for (int idx = 0; idx < a.Length; idx++)
            {
                if (rule.MutationChance)
                {
                    double mutated = a[idx].Connection.Weight + RandomGenerator.NextDouble(-rule.MutationStrength, rule.MutationStrength);
                    a[idx].Connection.Weight = new DoubleRange(-rule.WeightValueRange, rule.WeightValueRange).Cut(mutated);
                }
            }
        }

        private SortedSet<int> CreateCrossoverPoints()
        {
            int count = LearningConnections.Count;
            var set = new SortedSet<int>();
            while (set.Count != rule.CrossoverPoints)
            {
                set.Add(RandomGenerator.Random.Next(count));
            }
            return set;
        }

        private IEnumerable<double[]> SelectParents()
        {
            foreach (var idx in bestSel.Select(IntRange.CreateExclusive(0, population.Count), rule.ParentCountRange.PickRandomValue()))
            {
                yield return population.Values[idx];
            }
        }

        private void RemoveWorst()
        {
            int idx = worstSel.Select(IntRange.CreateExclusive(0, population.Size), 1).First();
            population.RemoveAt(idx);
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

        private void RandomizeWeights()
        {
            var a = LearningConnections.ItemArray;
            for (int idx = 0; idx < a.Length; idx++) a[idx].Connection.Weight = RandomGenerator.NextDouble(-rule.WeightValueRange, rule.WeightValueRange);
        }

        #endregion
    }
}
