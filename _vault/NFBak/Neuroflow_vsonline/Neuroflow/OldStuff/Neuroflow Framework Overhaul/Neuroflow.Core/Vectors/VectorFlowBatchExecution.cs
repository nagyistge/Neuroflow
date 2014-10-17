using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Core.Vectors
{
    [Serializable]
    public abstract class VectorFlowBatchExecution<T> : VectorFlowExecution<T>
        where T : struct
    {
        public VectorFlowBatchExecution(IComputationUnit<T> computationUnit, IterationRepeatPars repeatPars = null)
            : base(computationUnit)
        {
            Contract.Requires(computationUnit != null);

            RepeatPars = repeatPars;
        }

        public IterationRepeatPars RepeatPars { get; private set; }

        public BatchExecutionResult DoBatchExcuteIterations(VectorFlowBatch<T> batch, Action<BatchExecutionResult> iterationCallback = null)
        {
            Contract.Requires(batch != null);

            lock(SyncRoot)
            {
                return LockedDoBatchExcuteIterations(batch, iterationCallback);
            }
        }

        protected virtual BatchExecutionResult LockedDoBatchExcuteIterations(VectorFlowBatch<T> batch, Action<BatchExecutionResult> iterationCallback)
        {
            Contract.Requires(batch != null);
            Contract.Ensures(!Contract.Result<BatchExecutionResult>().IsEmpty);

            double[] resultErrors = new double[batch.vectorFlows.Length];

            int minIt, maxIt;
            if (RepeatPars != null)
            {
                minIt = RepeatPars.MinIterations;
                maxIt = RepeatPars.MaxIterations;
            }
            else
            {
                minIt = 1;
                maxIt = 1;
            }

            double bestError = double.MaxValue, currentError = double.MaxValue;
            for (int it = 1; it <= maxIt; it++)
            {
                currentError = ExecuteBatchIteration(batch, resultErrors);

                if (iterationCallback != null) iterationCallback(new BatchExecutionResult(resultErrors, currentError));

                if (it >= minIt && currentError >= bestError)
                {
                    break;
                }

                bestError = currentError;
            }

            return new BatchExecutionResult(resultErrors, currentError);
        }

        protected virtual double ExecuteBatchIteration(VectorFlowBatch<T> batch, double[] resultErrors)
        {
            double currentError = 0.0;
            for (int sidx = 0; sidx < batch.vectorFlows.Length; sidx++)
            {
                var vectorFlow = batch.vectorFlows[sidx];
                double vfError = ExecuteVectorFlow(vectorFlow);
                resultErrors[sidx] = vfError;
                currentError += vfError;
            }
            currentError /= (double)batch.vectorFlows.Length;
            return currentError;
        }
    }
}
