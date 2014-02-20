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
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "learningScriptEntry")]
    public sealed class LearningScriptEntry : ComputationScriptEntry<double>
    {
        public LearningScriptEntry(double?[] inputVector, double?[] desiredOutputVector, int numberOfIterations = 1)
            : base(inputVector, desiredOutputVector, numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(inputVector == null || inputVector.Length > 0);
            Contract.Requires(desiredOutputVector == null || desiredOutputVector.Length > 0);
        }
    }
}
