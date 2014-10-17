using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.Quantum;
using NeoComp.Networks;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.Learning
{
    internal sealed class BufferedQSAItem : IQuantumStatedItem
    {
        internal BufferedQSAItem(IAdjustableItem item)
        {
            Contract.Requires(item != null);

            this.item = item;
        }

        IAdjustableItem item;
        
        public QuantumState State { get; set; }

        internal void Apply()
        {
            item.Adjustment = State * 2.0 - 1.0; 
        }

        internal void Import()
        {
            State = (item.Adjustment + 1.0) / 2.0;
        }
    }
}
