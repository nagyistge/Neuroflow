using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "vectorFlowEntry")]
    public sealed class VectorFlowEntry<T>
        where T : struct
    {
        public VectorFlowEntry(T?[] inputVector, T?[] desiredOutputVector, int numberOfIterations = 1)
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
