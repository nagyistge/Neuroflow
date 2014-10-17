using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Logical;

namespace NeoComp.LogicalEvolution
{
    internal static class Definitions
    {
        internal static readonly LogicalOperation[] AllOps = new[] { LogicalOperation.AND, LogicalOperation.NAND, LogicalOperation.NOR, LogicalOperation.OR, LogicalOperation.XNOR, LogicalOperation.XOR };
    }
}
