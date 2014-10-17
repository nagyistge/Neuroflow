using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Adjustables
{
    public class RangedAdjustablePropertyValue : IRangedAdjustableItem
    {
        public RangedAdjustablePropertyValue(DoubleRange range, double value)
        {
            Contract.Requires(!range.IsFixed);
            Contract.Requires(range.IsIn(value));

            Range = range;
            this.value = value;
        }
        
        public DoubleRange Range { get; private set; }

        double value;

        public double Value
        {
            get { return value; }
            set
            {
                Contract.Requires(Range.IsIn(value));

                this.value = value;
            }
        }

        double IAdjustableItem.Adjustment
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
