using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Optimization.Learning
{
    public sealed class WeightDecayAlgorithm : ForwardLearningAlgorithm
    {
        protected internal override void InitializeNewRun()
        {
            // Do nothing.
        }

        protected internal override bool ForwardIteration(bool isNewBatch)
        {
            foreach (var lc in LearningConnections)
            {
                var conn = lc.Connection;
                var rule = (WeightDecayRule)lc.Rule;
                if (rule.UpdateOnEachVector)
                {
                    Update(conn, rule);
                }
                else if (isNewBatch)
                {
                    Update(conn, rule);
                }
            }
            
            return true; // Done.
        }

        private static void Update(INeuralConnection conn, WeightDecayRule rule)
        {
            double w4 = Math.Pow(conn.Weight, 4.0);
            double div = w4 + rule.Cutoff4;
            if (div != 0.0)
            {
                double delta = (w4 / div) * rule.Factor * conn.Weight;
                conn.Weight += delta;
            }
        }
    }
}
