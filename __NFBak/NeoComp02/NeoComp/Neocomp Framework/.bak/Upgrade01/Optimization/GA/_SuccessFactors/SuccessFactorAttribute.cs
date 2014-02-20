using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.GA
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SuccessFactorAttribute : Attribute
    {
        public SuccessFactorAttribute()
            : this(0)
        {
        }
        
        public SuccessFactorAttribute(int order)
            : this(order, ComparationMode.None)
        {
        }

        public SuccessFactorAttribute(int order, ComparationMode comparationMode)
        {
            Order = order;
            ComparationMode = comparationMode;
        }

        public int Order { get; private set; }

        public ComparationMode ComparationMode { get; private set; }
    }
}
