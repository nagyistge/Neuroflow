using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Logical
{
    public sealed class LogicalNetworkTestResult : ComputationTestResult<bool, bool?>
    {
        internal LogicalNetworkTestResult(Guid testUID, IList<ComputationTestResultEntry<bool, bool?>> entryList)
            : base(testUID, entryList)
        {
            Contract.Requires(!entryList.IsNullOrEmpty());
        }

        protected override double GetError(bool computedValue, bool? desiredValue)
        {
            return desiredValue.HasValue ? (desiredValue.Value == computedValue ? 0.0 : 1.0) : 0.0;
        }
    }
}
