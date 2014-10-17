using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.Learning
{
    public interface IDeltaBasedAdjustable : IAdjustableItem
    {
        double Delta { get; set; }
    }
}
