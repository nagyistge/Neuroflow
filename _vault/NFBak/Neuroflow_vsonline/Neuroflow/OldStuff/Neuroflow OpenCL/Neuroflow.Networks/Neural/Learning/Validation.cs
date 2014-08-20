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
            : base(network, null)
        {
            Contract.Requires(network != null);
        }

        protected override double LockedExecuteVectorFlow(Core.Vectors.VectorFlow<float> vectorFlow)
        {
            if (Network.IsRecurrent) Network.Reset(NeuralNetworkResetTarget.Outputs);
            return base.LockedExecuteVectorFlow(vectorFlow);
        }
    }
}
