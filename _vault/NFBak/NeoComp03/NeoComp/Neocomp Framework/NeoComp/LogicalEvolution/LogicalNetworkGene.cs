using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.LogicalEvolution
{
    public abstract class LogicalNetworkGene
    {
        protected LogicalNetworkGene(int index)
        {
            Index = index;
        }
        
        public int Index { get; private set; }
    }
}
