using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Neuroflow.Core.Vectors
{
    [Serializable]
    public abstract class SupervisedBatchExecution<T> : SupervisedExecution<T>
        where T : struct
    {
        public SupervisedBatchExecution(IComputationUnit<T> computationUnit, int iterationRepeat)
            : base(computationUnit)
        {
            Contract.Requires(computationUnit != null);
            Contract.Requires(iterationRepeat > 0);

            IterationRepeat = iterationRepeat;
        }

        public int IterationRepeat { get; private set; }

        public void ExcuteBatch(VectorComputationContext context, VectorBuffer<T> vectorBuffer, VectorFlowBatch<T> batch)
        {
            Contract.Requires(context != null);
            Contract.Requires(batch != null);

            lock (context.SyncRoot)
            {
                lock (vectorBuffer.SyncRoot)
                {
                    DoExecuteBatch(context, vectorBuffer, batch);
                }
            }
        }

        protected virtual void DoExecuteBatch(VectorComputationContext context, VectorBuffer<T> vectorBuffer, VectorFlowBatch<T> batch)
        {
            Contract.Requires(batch != null);

            for (int i = 0; i < IterationRepeat; i++) ExecuteBatchIteration(context, vectorBuffer, batch);
        }

        protected virtual void ExecuteBatchIteration(VectorComputationContext context, VectorBuffer<T> vectorBuffer, VectorFlowBatch<T> batch)
        {
            for (int sidx = 0; sidx < batch.vectorFlows.Length; sidx++)
            {
                var vectorFlow = batch.vectorFlows[sidx];
                ExecuteVectorFlow(context, vectorBuffer, vectorFlow);
            }
        }
    }
}
