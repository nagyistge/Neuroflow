using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Neuroflow.Core.Serialization;

namespace Neuroflow.Core.Vectors
{
    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public class VectorFlowBatch<T>
        where T : struct
    {
        public VectorFlowBatch(VectorFlow<T> vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            this.vectorFlows = new[] { vectorFlow };
        }

        public VectorFlowBatch(VectorFlow<T>[] vectorFlows)
        {
            Contract.Requires(vectorFlows != null && vectorFlows.Length > 0);

            this.vectorFlows = vectorFlows;
        }

        [DataMember(Name = "VectorFlows")]
        internal VectorFlow<T>[] vectorFlows;

        public ReadOnlyCollection<VectorFlow<T>> VectorFlows
        {
            get { return Array.AsReadOnly(vectorFlows); }
        }
    }
}
