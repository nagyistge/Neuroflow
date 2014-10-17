using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Computations;

namespace Neuroflow.Core.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "vectorFlowBatchExec")]
    public abstract class VectorFlowBatchExecution<T> : VectorFlowExecution<T>
        where T : struct
    {
        public VectorFlowBatchExecution(IComputationUnit<T> computationUnit, double errorScale, IterationRepeatPars repeatPars = null)
            : base(computationUnit)
        {
            Contract.Requires(computationUnit != null);
            Contract.Requires(errorScale > 0.0);

            ErrorScale = errorScale;
            RepeatPars = repeatPars;
        }

        [DataMember(Name = "errorScale")]
        public double ErrorScale { get; private set; }

        [DataMember(Name = "itRepPars", EmitDefaultValue = false)]
        public IterationRepeatPars RepeatPars { get; private set; }

        public BatchExecutionResult Execute(VectorFlowBatch<T> batch, Action<BatchExecutionResult> iterationCallback = null)
        {
            Contract.Requires(batch != null);

            using (new ComputationUnitGuard(this))
            {
                return BatchExecution(batch, iterationCallback);
            }
        }

        protected virtual BatchExecutionResult BatchExecution(VectorFlowBatch<T> batch, Action<BatchExecutionResult> iterationCallback)
        {
            Contract.Requires(batch != null);
            Contract.Ensures(Contract.Result<BatchExecutionResult>() != null);

            double[] resultErrors = new double[batch.Count];

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
                currentError = ExecuteBatch(batch, resultErrors);

                if (iterationCallback != null) iterationCallback(new BatchExecutionResult(resultErrors, currentError));

                if (it >= minIt && currentError >= bestError)
                {
                    break;
                }

                bestError = currentError;
            }

            return new BatchExecutionResult(resultErrors, currentError);
        }

        protected virtual double ExecuteBatch(VectorFlowBatch<T> batch, double[] resultErrors)
        {
            double currentError = 0.0;
            for (int sidx = 0; sidx < batch.Count; sidx++)
            {
                var vectorFlow = batch[sidx];
                double vfError = base.Excute(vectorFlow);
                resultErrors[sidx] = vfError;
                currentError += vfError;
            }
            currentError /= (double)batch.Count;
            return currentError;
        }
    }
}
