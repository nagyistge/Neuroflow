using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Logical;

namespace NeoComp.LogicalEvolution
{
    public class LogicalConnectionGene : LogicalNetworkGene
    {
        public LogicalConnectionGene(int index, bool isUpper)
            : base(index)
        {
            IsUpper = isUpper;
        }

        public bool IsUpper { get; private set; }
        
        protected internal virtual LogicalConnection CreateConnection()
        {
            Contract.Ensures(Contract.Result<LogicalConnection>() != null);
            
            return new LogicalConnection();
        }
    }
}
