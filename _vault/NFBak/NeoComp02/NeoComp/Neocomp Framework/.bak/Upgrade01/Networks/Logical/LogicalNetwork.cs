using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;

namespace NeoComp.Networks.Logical
{
    public class LogicalNetwork : ComputationalNetwork<LogicalConnection, bool>
    {
        #region Constructor

        public LogicalNetwork(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
        } 

        #endregion
    }
}
