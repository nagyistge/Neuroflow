using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Neuroflow.Core.Serialization;

namespace Neuroflow.Core.Vectors
{
    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public struct BatchExecutionResult
    {
        public BatchExecutionResult(float[] errors, float averageError)
        {
            Contract.Requires(errors != null && errors.Length != 0);
            Contract.Requires(averageError >= 0.0);

            this.errors = errors;
            this.averageError = averageError;
        }

        public bool IsEmpty
        {
            [Pure]
            get { return this.errors == null; }
        }

        [DataMember(Name = "Errors")]
        float[] errors;

        public float[] Errors
        {
            get { return errors; }
        }

        [DataMember(Name = "AverageError")]
        float averageError;

        public float AverageError
        {
            get { return averageError; }
        }

        public override string ToString()
        {
            return averageError.ToString();
        }
    }
}
