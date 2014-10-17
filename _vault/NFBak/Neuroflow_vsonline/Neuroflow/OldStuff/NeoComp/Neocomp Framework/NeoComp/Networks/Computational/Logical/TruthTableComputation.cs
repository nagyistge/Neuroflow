using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public sealed class TruthTableComputation : ComputationScriptExecution<bool>
    {
        public TruthTableComputation(LogicalNetwork network)
            : base(network)
        {
        }

        public LogicalNetwork Network
        {
            get { return (LogicalNetwork)base.ComputationUnit; }
        }

        public int ComputeError(TruthTable truthTable)
        {
            Contract.Requires(truthTable != null);

            double error = base.Excute(truthTable);

            return (int)(error * (double)truthTable.ItemArray.Length);
        }

        protected override double ComputeError(bool?[] desiredOutputVector)
        {
            int numberOfErrors = 0;
            for (int idx = 0; idx < desiredOutputVector.Length; idx++)
            {
                bool? desiredValue = desiredOutputVector[idx];
                if (desiredValue != null)
                {
                    bool actualValue = Network.OutputInterface[idx];
                    if (actualValue != desiredValue.Value) numberOfErrors++;
                }
            }
            return numberOfErrors;
        }
    }
}
