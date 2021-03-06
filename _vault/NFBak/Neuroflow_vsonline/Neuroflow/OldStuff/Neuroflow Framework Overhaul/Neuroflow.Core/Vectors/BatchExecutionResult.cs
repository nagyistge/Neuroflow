﻿using System;
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
        public BatchExecutionResult(double[] errors, double averageError)
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
        double[] errors;

        public double[] Errors
        {
            get { return errors; }
        }

        [DataMember(Name = "AverageError")]
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
