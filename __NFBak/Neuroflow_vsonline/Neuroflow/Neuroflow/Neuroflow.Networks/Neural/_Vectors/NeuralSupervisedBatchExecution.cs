using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Learning;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    [Serializable]
    public abstract class NeuralSupervisedBatchExecution : SupervisedBatchExecution<float>
    {
        protected NeuralSupervisedBatchExecution(NeuralNetwork network, int iterationRepeat, BufferAllocator allocator)
            : base(network, iterationRepeat)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);
            Contract.Requires(iterationRepeat > 0);

            Network = network;
            LastErrorBuffer = allocator.Alloc(network.OutputInterfaceLength);
            ErrorAccumulationBuffer = allocator.Alloc(network.OutputInterfaceLength + 1);
            AverageErrorBuffer = IntRange.CreateInclusive(ErrorAccumulationBuffer.MinValue, ErrorAccumulationBuffer.MaxValue - 1);
        }

        public NeuralNetwork Network { get; private set; }

        public IntRange LastErrorBuffer { get; private set; }

        public IntRange AverageErrorBuffer { get; private set; }

        public IntRange ErrorAccumulationBuffer { get; private set; }

        protected override void ExecuteBatchIteration(VectorComputationContext context, VectorBuffer<float> vectorBuffer, VectorFlowBatch<float> batch)
        {
            var ctx = (NeuralComputationContext)context;

            Network.ZeroBuffer(ctx, ErrorAccumulationBuffer);

            base.ExecuteBatchIteration(context, vectorBuffer, batch);

            Network.CalculateAverageError(ctx, ErrorAccumulationBuffer);
        }

        protected unsafe override void ComputeError(VectorComputationContext context, BufferedVector<float> desiredOutputVector, int entryIndex, int entryCount)
        {
            Debug.Assert(LastErrorBuffer.Size == desiredOutputVector.Length);
            Debug.Assert(Network.OutputInterfaceLength == desiredOutputVector.Length);
            Debug.Assert(context is NeuralComputationContext);

            Network.ComputeError((NeuralComputationContext)context, desiredOutputVector, LastErrorBuffer, ErrorAccumulationBuffer);
        }

        #region Public

        public void ReadError(NeuralComputationContext context, float[] values)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == AverageErrorBuffer.Size);

            Network.ReadError(context, values, AverageErrorBuffer);
        }

        #endregion
    }
}
