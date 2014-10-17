using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public class IntegerOptUnit : DiscreteOptUnit<int>
    {
        public IntegerOptUnit(string id, int resolution, IntRange valueRange)
            : base(id, resolution)
        {
            Contract.Requires(!valueRange.IsFixed);
            Contract.Requires(resolution > 1);
            Contract.Requires(!string.IsNullOrEmpty(id)); 
            
            ValueRange = valueRange;
        }

        public IntRange ValueRange { get; private set; }

        protected override int CreateRandomValue()
        {
            return ValueRange.PickRandomValue();
        }

        protected override int CreateRandomValueExcept(HashSet<int> items)
        {
            int v = ValueRange.PickRandomValue();
            while (items.Contains(v))
            {
                v = ValueRange.PickRandomValue();
            }
            return v;
        }
    }
}
