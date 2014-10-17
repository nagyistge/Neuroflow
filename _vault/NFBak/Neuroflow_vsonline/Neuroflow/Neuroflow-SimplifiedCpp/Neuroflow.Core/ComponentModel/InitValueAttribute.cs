using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.ComponentModel
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class InitValueAttribute : Attribute
    {
        public InitValueAttribute(object value)
        {
            Value = value;
        }
        
        public object Value { get; private set; }
    }
}
