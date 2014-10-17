using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public struct ScriptBatchExecutionResult
    {
        public ScriptBatchExecutionResult(double[] errors, double averageError)
        {
            Contract.Requires(errors != null && errors.Length != 0);
            
            this.errors = errors;
            this.averageError = averageError;
        }
        
        public bool IsEmpty
        {
            [Pure]
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
    }
}
