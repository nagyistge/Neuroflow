using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "neuralVectors")]
    public sealed class NeuralVectors : VectorFlow<double>
    {
        public NeuralVectors(int providerIndex, VectorFlowEntry<double> entry)
            : base(entry)
        {
            Contract.Requires(entry != null);

            ProviderIndex = providerIndex;
        }

        public NeuralVectors(int providerIndex, IList<VectorFlowEntry<double>> entryList)
            : base(entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);

            ProviderIndex = providerIndex;
        }

        public NeuralVectors(int providerIndex, IEnumerable<VectorFlowEntry<double>> entryColl)
            : base(entryColl)
        {
            Contract.Requires(entryColl != null);

            ProviderIndex = providerIndex;
        }

        [DataMember]
        public int ProviderIndex { get; private set; }

        [DataMember]
        public bool IsEndOfStream { get; set; }
    }
}
