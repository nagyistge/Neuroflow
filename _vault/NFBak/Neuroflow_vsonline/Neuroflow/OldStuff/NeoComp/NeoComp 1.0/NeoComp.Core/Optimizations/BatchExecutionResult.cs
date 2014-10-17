using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "batchExResult")]
    public struct BatchExecutionResult
    {
        public BatchExecutionResult(double[] errors, double averageError)
        {
            Contract.Requires(errors != null && errors.Length != 0);
            Contract.Requires(averageError >= 0.0);

            this.errors = errors;
            this.averageError = averageError;
        }

        public static readonly BatchExecutionResult Empty = new BatchExecutionResult { averageError = double.MaxValue };

        [Pure]
        public bool IsEmpty
        {
            get { return errors == null || errors.Length == 0; }
        }

        double[] errors;

        public double[] Errors
        {
            get { return errors; }
        }

        double averageError;

        public double AverageError
        {
            get { return averageError; }
        }

        public override string ToString()
        {
            return averageError.ToString();
        }
    }
}
