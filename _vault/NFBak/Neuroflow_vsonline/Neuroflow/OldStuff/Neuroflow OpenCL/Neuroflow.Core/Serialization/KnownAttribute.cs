using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class KnownAttribute : Attribute
    {
    }
}
