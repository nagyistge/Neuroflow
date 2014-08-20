using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NeoComp.Computations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "compVal")]
    public sealed class ComputationValue<T>
        where T : struct
    {
        [DataMember(Name = "value")]
        public T Value { get; set; }
    }
}
