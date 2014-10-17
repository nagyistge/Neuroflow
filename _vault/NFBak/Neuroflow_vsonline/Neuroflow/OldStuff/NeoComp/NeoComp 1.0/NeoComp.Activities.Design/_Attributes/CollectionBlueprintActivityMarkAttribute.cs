using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities.Design
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CollectionBlueprintActivityMarkAttribute : Attribute
    {
    }
}
