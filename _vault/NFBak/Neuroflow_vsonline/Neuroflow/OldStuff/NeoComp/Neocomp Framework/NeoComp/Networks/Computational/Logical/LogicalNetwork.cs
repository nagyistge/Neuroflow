using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public class LogicalNetwork : ComputationalNetwork<bool>
    {
        public LogicalNetwork(LogicalNetworkFactory factory)
            : base(factory)
        {
            Contract.Requires(factory != null);

            LogicGateRestrictions = factory.LogicGateRestrictions;
        }

        #region Properties

        public LogicGateTypes LogicGateRestrictions { get; private set; }

        #endregion

        #region Clone

        new public LogicalNetwork Clone()
        {
            return (LogicalNetwork)base.Clone();
        }

        #endregion
    }
}
