using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.GA
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class SuccessFactorContainerAttribute : Attribute
    {
    }
}
