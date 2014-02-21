using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class BackProgationContext
    {
        internal BackProgationContext(object ruleContext)
        {
            RuleContext = ruleContext;
        }

        public object RuleContext { get; private set; }
    }
}
