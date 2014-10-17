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
    public sealed class VectorFlowEntry<T>
        where T : struct
    {
        public VectorFlowEntry(params T[][] vectors) :
            this(1, vectors)
        {
            Contract.Requires(vectors != null);
            Contract.Requires(vectors.Length > 0);
        }

        public VectorFlowEntry(int numberOfIterations, params T[][] vectors)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(vectors != null);
            Contract.Requires(vectors.Length > 0);

            NumberOfIterations = numberOfIterations;
            Vectors = vectors;
        }

        [DataMember]
        public int NumberOfIterations { get; private set; }

        [DataMember]
        public T[][] Vectors { get; private set; }
    }
}
