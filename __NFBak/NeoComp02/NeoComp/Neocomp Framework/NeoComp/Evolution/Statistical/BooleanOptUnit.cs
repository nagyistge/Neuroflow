using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public class BooleanOptUnit : DiscreteOptUnit<bool>
    {
        public BooleanOptUnit(string id)
            : base(id, 2)
        {
            Contract.Requires(!string.IsNullOrEmpty(id)); 
        }

        protected override bool CreateRandomValue()
        {
            return RandomGenerator.Random.Next(2) == 0;
        }

        protected override bool CreateRandomValueExcept(HashSet<bool> items)
        {
            throw new InvalidOperationException("Internal error: this method should not be called.");
        }
    }
}
