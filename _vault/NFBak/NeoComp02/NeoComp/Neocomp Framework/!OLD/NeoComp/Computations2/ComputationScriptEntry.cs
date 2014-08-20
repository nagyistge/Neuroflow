using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Computations2
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compScriptEntry")]
    public class ComputationScriptEntry<T>
        where T : struct
    {
        public ComputationScriptEntry(T?[] inputVector, T?[] desiredOutputVector, int numberOfIterations = 1)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(inputVector == null || inputVector.Length > 0);
            Contract.Requires(desiredOutputVector == null || desiredOutputVector.Length > 0);

            NumberOfIterations = numberOfIterations;
            InputVector = inputVector;
            DesiredOutputVector = desiredOutputVector;
        }

        [DataMember(Name = "numOfIts")]
        public int NumberOfIterations { get; private set; }

        [DataMember(Name = "input")]
        public T?[] InputVector { get; private set; }

        [DataMember(Name = "output")]
        public T?[] DesiredOutputVector { get; private set; }
    }
}
