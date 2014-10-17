using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Neuroflow.Core.Serialization;

namespace Neuroflow.Networks.Neural
{
    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class NeuralVectorFlow : VectorFlow<double>
    {
        public NeuralVectorFlow(int providerIndex, VectorFlowEntry<double> entry, bool isEndOfStream = false)
            : base(entry)
        {
            Contract.Requires(entry != null);

            ProviderIndex = providerIndex;
            IsEndOfStream = isEndOfStream;
        }

        public NeuralVectorFlow(int providerIndex, VectorFlowEntry<double>[] entries, bool isEndOfStream = false)
            : base(entries)
        {
            Contract.Requires(entries != null);
            Contract.Requires(entries.Length > 0);

            ProviderIndex = providerIndex;
            IsEndOfStream = isEndOfStream;
        }

        [DataMember]
        public int ProviderIndex { get; private set; }

        [DataMember]
        public bool IsEndOfStream { get; private set; }
    }
}
