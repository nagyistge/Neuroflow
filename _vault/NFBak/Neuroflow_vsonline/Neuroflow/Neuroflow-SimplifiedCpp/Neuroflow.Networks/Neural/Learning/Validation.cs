using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class Validation : NeuralSupervisedBatchExecution
    {
        internal Validation(NeuralNetwork network, BufferAllocator allocator)
            : base(network, 1, allocator)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);
        }

        protected override void DoExecuteVectorFlow(VectorComputationContext context, VectorBuffer<float> vectorBuffer, Core.Vectors.VectorFlow<float> vectorFlow)
        {
            var ctx = (NeuralComputationContext)context;
            if (Network.IsRecurrent) Network.Reset(ctx, NeuralNetworkResetTarget.Outputs);
            base.DoExecuteVectorFlow(context, vectorBuffer, vectorFlow);
        }
    }
}
