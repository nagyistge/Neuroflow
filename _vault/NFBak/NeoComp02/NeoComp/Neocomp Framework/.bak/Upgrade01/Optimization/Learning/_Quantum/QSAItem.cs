using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Quantum;
using NeoComp.Networks;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.Learning
{
    internal sealed class QSAItem : IQuantumStatedItem
    {
        internal QSAItem(IAdjustableItem item)
        {
            Contract.Requires(item != null);

            this.item = item;
        }

        IAdjustableItem item;

        QuantumState IQuantumStatedItem.State
        {
            get { return (item.Adjustment + 1.0) / 2.0; }
            set { item.Adjustment = value * 2.0 - 1.0; }
        }
    }
}
