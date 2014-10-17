using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class WeightDecayAlgorithm : ForwardLearningAlgorithm
    {
        protected internal override void InitializeNewRun(AlgoInitializationMode mode) { }

        protected internal override void ForwardIteration(bool isNewBatch)
        {
            foreach (var lc in LearningConnections)
            {
                var conn = lc.Connection;
                var rule = (WeightDecayRule)lc.Rule;
                if (isNewBatch)
                {
                    Update(conn, rule);
                }
            }
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
