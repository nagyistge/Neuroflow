using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Computations;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class OperationNode<T> : ComputationalNode<T>
        where T : struct
    {
        [DataMember(Name = "output")]
        public ComputationValue<T> OutputValue { get; private set; }
        
        protected internal sealed override void Computation(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
        {
            if (outputConnectionEntries.Length > 0)
            {
                OutputValue.Value = GenerateOutput(inputConnectionEntries);
            }
        }

        protected internal override void Initialize(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
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

        protected abstract T GenerateOutput(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries);

        protected override void Reset()
        {
            OutputValue.Value = default(T);
        }
    }
}
