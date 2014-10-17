using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColorTest;

namespace MixColor
{
    public sealed class WeightedBaseColor : WeightedValue<BaseColor>
    {
        public WeightedBaseColor(BaseColor baseColor, double weight) :
            base(baseColor, weight)
        {
        }
    }
}
