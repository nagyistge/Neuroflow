using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;

namespace NeoComp.Networks.Logical
{
    public class LogicalConnection : ComputationalConnection<bool>
    {
        public bool BitValue
        {
            get { return ConnectionValue.Value; }
        }
    }
}
