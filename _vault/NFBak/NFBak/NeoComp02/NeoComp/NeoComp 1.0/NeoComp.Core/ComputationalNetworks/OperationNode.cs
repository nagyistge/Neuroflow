using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp;
using NeoComp.Computations;
using System.Runtime.Serialization;
using NeoComp.Networks;

namespace NeoComp.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS)]
    public abstract class OperationNode<T> : ComputationNode<T>
        where T : struct
    {
        [DataMember(Name = "output")]
        public ComputationValue<T> OutputValue { get; private set; }
        
        protected internal sealed override void Computation(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationConnection<T>>[] outputConnectionEntries)
        {
            if (outputConnectionEntries.Length > 0)
            {
                OutputValue.Value = GenerateOutput(inputConnectionEntries);
            }
        }

        protected internal override void Initialize(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationConnection<T>>[] outputConnectionEntries)
        {
            foreach (var oe in outputConnectionEntries)
            {
                if (oe.Connection.ConnectionValue != null)
                {
                    // Adapted from interface:
                    OutputValue = oe.Connection.ConnectionValue;
                    return;
                }
                else
                {
                    if (OutputValue == null) OutputValue = new ComputationValue<T>();
                    oe.Connection.AdaptValue(OutputValue);
                }
            }
        }

        protected abstract T GenerateOutput(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries);

        protected override void Reset()
        {
            OutputValue.Value = default(T);
        }
    }
}
