using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultExpressionAttribute : Attribute
    {
        public DefaultExpressionAttribute(string expression)
        {
            Expression = expression;
        }
        
        public string Expression { get; private set; }
    }
}
