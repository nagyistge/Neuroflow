using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities.Design
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DeclarationAttribute : Attribute
    {
        public DeclarationAttribute(string variableName)
        {
            VariableName = variableName;
        }
        
        public string VariableName { get; private set; }
    }
}
