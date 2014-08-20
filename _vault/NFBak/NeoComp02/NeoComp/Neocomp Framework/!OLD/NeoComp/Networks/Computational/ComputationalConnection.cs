using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class ComputationalConnection<T>
    {
        [DataMember(Name = "value", EmitDefaultValue = false)]
        internal ComputationalValue<T> ConnectionValue { get; private set; }

        public T InputValue
        {
            get { return ConnectionValue.Value; }
        }

        public virtual T OutputValue
        {
            get { return InputValue; }
        }

        internal void AdaptValue(ComputationalValue<T> value)
        {
            Contract.Requires(value != null);

            ConnectionValue = value;
        }
    }
}
