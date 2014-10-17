using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations2;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "learningScriptBatchExec")]
    public class LearningScriptBatchExecution : ComputationScriptExecution<double>
    {
        public LearningScriptBatchExecution(IComputationUnit<double> computationUnit, double errorScale, IterationRepeatPars repeatPars = null)
            : base(computationUnit)
        {
            Contract.Requires(computationUnit != null);
            Contract.Requires(errorScale > 0.0);

            ErrorScale = errorScale;
            RepeatPars = repeatPars;
        }

        public double ErrorScale { get; private set; }

        public IterationRepeatPars RepeatPars { get; private set; }

        public ScriptBatchExecutionResult Execute(LearningScriptBatch scriptBatch)
        {
            Contract.Requires(scriptBatch != null);

            using (new ComputationUnitGuard(this))
            {
                return GuardedExecute(scriptBatch);
            }
        }

        protected virtual ScriptBatchExecutionResult GuardedExecute(LearningScriptBatch scriptBatch)
        {
            Contract.Requires(scriptBatch != null);
            Contract.Ensures(!Contract.Result<ScriptBatchExecutionResult>().IsEmpty);

            double lastError = double.MaxValue;
            double[] resultErrors = new double[scriptBatch.Count];
            for (int it = 0; it < RepeatPars.MaxIterations; it++)
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

                if (it >= RepeatPars.MinIterations)
                {
                    if (currentError < lastError)
                    {
                        lastError = currentError;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return new ScriptBatchExecutionResult(resultErrors, lastError);
        }

        protected override double ComputeError(double?[] desiredOutputVector)
        {
            double sum = 0.0;
            for (int idx = 0; idx < desiredOutputVector.Length; idx++)
            {
                double? desiredValue = desiredOutputVector[idx];
                if (desiredValue.HasValue)
                {
                    double difference = ComputationUnit.OutputInterface[idx] - desiredValue.Value;
                    RegiterErrorDifference(idx, difference);
                    sum += Math.Sqrt(difference * ErrorScale);
                }
            }
            return sum / (double)desiredOutputVector.Length;
        }

        protected virtual void RegiterErrorDifference(int index, double difference)
        {
            Contract.Requires(index >= 0 && index < ComputationUnit.OutputInterface.Length);
        }
    }
}
