using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.Learning
{
    [Serializable]
    public sealed class Validation : NeuralVectorFlowBatchExecution
    {
        public Validation(NeuralNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);
        }

        protected override double LockedExecuteVectorFlow(Core.Vectors.VectorFlow<double> vectorFlow)
        {
            if (Network.IsRecurrent) Network.Reset();
            return base.LockedExecuteVectorFlow(vectorFlow);
        }
    }
}
