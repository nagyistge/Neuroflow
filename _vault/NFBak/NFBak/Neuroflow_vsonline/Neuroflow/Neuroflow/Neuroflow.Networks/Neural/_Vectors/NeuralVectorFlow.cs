using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Neuroflow.Core.Serialization;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class NeuralVectorFlow : VectorFlow<float>
    {
        public NeuralVectorFlow(int index, VectorFlowEntry<float> entry, bool isEndOfStream = false)
            : base(index, entry)
        {
            Contract.Requires(entry != null);

            IsEndOfStream = isEndOfStream;
        }

        public NeuralVectorFlow(int index, VectorFlowEntry<float>[] entries, bool isEndOfStream = false)
            : base(index, entries)
        {
            Contract.Requires(entries != null);
            Contract.Requires(entries.Length > 0);

            IsEndOfStream = isEndOfStream;
        }

        [DataMember]
        public bool IsEndOfStream { get; private set; }
    }
}
