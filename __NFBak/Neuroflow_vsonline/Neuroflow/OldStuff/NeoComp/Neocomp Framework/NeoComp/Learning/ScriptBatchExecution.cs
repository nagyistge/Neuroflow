using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "scriptBatchExec")]
    public class ScriptBatchExecution : ComputationScriptExecution<double>
    {
        public ScriptBatchExecution(IComputationUnit<double> computationUnit, double errorScale, IterationRepeatPars repeatPars = null)
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

        public ScriptBatchExecutionResult Execute(ScriptBatch scriptBatch, Action<ScriptBatchExecutionResult> iterationCallback = null)
        {
            Contract.Requires(scriptBatch != null);

            using (new ComputationUnitGuard(this))
            {
                return BatchExecution(scriptBatch, iterationCallback);
            }
        }

        protected virtual ScriptBatchExecutionResult BatchExecution(ScriptBatch scriptBatch, Action<ScriptBatchExecutionResult> iterationCallback)
        {
            Contract.Requires(scriptBatch != null);
            Contract.Ensures(!Contract.Result<ScriptBatchExecutionResult>().IsEmpty);

            double[] resultErrors = new double[scriptBatch.Count];
            
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
                currentError = ExecuteBatch(scriptBatch, resultErrors);

                if (it >= minIt && currentError >= bestError)
                {
                    break;
                }

                bestError = currentError;
                if (iterationCallback != null) iterationCallback(new ScriptBatchExecutionResult(resultErrors, currentError));
            }
            return new ScriptBatchExecutionResult(resultErrors, currentError);

            //if (maxIt == 1)
            //{
            //    double currentError = ExecuteBatch(scriptBatch, resultErrors);
            //    return new ScriptBatchExecutionResult(resultErrors, currentError);
            //}
            //else
            //{
            //    double bestError = double.MaxValue, currentError = double.MaxValue;
            //    double[] sumResultErrors = new double[scriptBatch.Count];
            //    double dCount = 0.0;
            //    for (int it = 1; it <= maxIt; it++, dCount++)
            //    {
            //        currentError = ExecuteBatch(scriptBatch, resultErrors);

            //        for (int idx = 0; idx < resultErrors.Length; idx++)
            //        {
            //            sumResultErrors[idx] += resultErrors[idx];
            //        }

            //        if (it >= minIt && currentError >= bestError)
            //        {
            //            break;
            //        }

            //        bestError = currentError;
            //        if (iterationCallback != null) iterationCallback(new ScriptBatchExecutionResult(resultErrors, currentError));
            //    }
            //    for (int idx = 0; idx < sumResultErrors.Length; idx++) sumResultErrors[idx] /= dCount;
            //    var result = new ScriptBatchExecutionResult(sumResultErrors, sumResultErrors.Average());
            //    return result;
            //}
        }

        protected virtual double ExecuteBatch(ScriptBatch scriptBatch, double[] resultErrors)
        {
            double currentError = 0.0;
            for (int sidx = 0; sidx < scriptBatch.Count; sidx++)
            {
                var script = scriptBatch[sidx];
                double scriptError = base.Excute(script);
                resultErrors[sidx] = scriptError;
                currentError += scriptError;
            }
            currentError /= (double)scriptBatch.Count;
            return currentError;
        }

        protected override double ComputeError(double?[] desiredOutputVector)
        {
            double count = 0.0;
            double sum = 0.0;
            for (int idx = 0; idx < desiredOutputVector.Length; idx++)
            {
                double? desiredValue = desiredOutputVector[idx];
                if (desiredValue.HasValue)
                {
                    double difference = desiredValue.Value - ComputationUnit.OutputInterface[idx];
                    RegiterErrorDifference(idx, desiredValue.Value, ComputationUnit.OutputInterface[idx]);
                    sum += Math.Pow(difference * ErrorScale, 2.0);
                    count++;
                }
            }
            return count != 0.0 ? (sum / count) / 2.0 : 0.0;
        }

        protected virtual void RegiterErrorDifference(int index, double desiredValue, double currentValue)
        {
            Contract.Requires(index >= 0 && index < ComputationUnit.OutputInterface.Length);
        }
    }
}
