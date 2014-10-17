using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Adjustables;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public sealed class QuantumStatedAdjustableItem : IQuantumStatedItem, IAdjustableItem
    {
        public QuantumStatedAdjustableItem(IAdjustableItem item)
        {
            Contract.Requires(item != null);

            this.item = item;
            var limited = item as IRangedAdjustableItem;
            double max;
            if (limited != null)
            {
                min = limited.Range.MinValue;
                max = limited.Range.MaxValue;
            }
            else
            {
                min = -1.0;
                max = 1.0;
            }
            size = max - min;
        }

        double min, size;

        IAdjustableItem item;

        QuantumState IQuantumStatedItem.State
        {
            get { return (item.Adjustment - min) / size; }
            set { item.Adjustment = value * size + min; }
        }

        double IAdjustableItem.Adjustment
        {
            get { return item.Adjustment; }
            set { item.Adjustment = value; }
        }
    }
}
