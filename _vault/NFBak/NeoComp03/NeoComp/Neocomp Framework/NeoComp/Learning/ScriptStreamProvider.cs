using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public abstract class ScriptStreamProvider : ScriptProvider
    {
        public abstract Script GetNext();
    }
}
