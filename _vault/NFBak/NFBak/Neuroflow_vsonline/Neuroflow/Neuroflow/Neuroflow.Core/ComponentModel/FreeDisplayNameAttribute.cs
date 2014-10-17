using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Neuroflow.Core.ComponentModel
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class FreeDisplayNameAttribute : DisplayNameAttribute
    {
        public FreeDisplayNameAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
