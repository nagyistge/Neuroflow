using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ActivityDelegateMetadataAttribute : Attribute
    {
        public ActivityDelegateMetadataAttribute()
        {
            ResultName = "Result";
            Argument1Name = "Argument1";
            Argument2Name = "Argument2";
            Argument3Name = "Argument3";
            Argument4Name = "Argument4";
            Argument5Name = "Argument5";
            Argument6Name = "Argument6";
            Argument7Name = "Argument7";
            Argument8Name = "Argument8";
            Order = int.MaxValue;
        }

        public int Order { get; set; }

        public string ObjectName { get; set; }

        public string ResultName { get; set; }

        public string Argument1Name { get; set; }

        public string Argument2Name { get; set; }

        public string Argument3Name { get; set; }

        public string Argument4Name { get; set; }

        public string Argument5Name { get; set; }

        public string Argument6Name { get; set; }

        public string Argument7Name { get; set; }

        public string Argument8Name { get; set; }
    }
}
