using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Activities.Design;

namespace NeoComp.Activities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BlueprintFuncAttribute : OrderAttribute
    {
        public string ResultVariableName { get; set; }
    }
}
