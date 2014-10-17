using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities.Design
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OrderAttribute : Attribute
    {
        public OrderAttribute()
        {
            Order = int.MaxValue;
        }

        public OrderAttribute(int order)
        {
            Order = order;
        }
        
        public int Order { get; set; }
    }
}
