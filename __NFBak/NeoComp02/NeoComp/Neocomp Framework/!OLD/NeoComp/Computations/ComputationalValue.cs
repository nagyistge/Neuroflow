using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NeoComp.Computations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compValue")]
    public sealed class ComputationalValue<T>
    {
        public ComputationalValue()
        {
        }

        public ComputationalValue(T value)
        {
            Value = value;
        }
        
        [DataMember(Name = "data")]
        public T Value { get; set; }
    }
}
