using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NeoComp.Core
{
    public static class CancellationHelperExtensions
    {
        public static bool IsCancellationRequested(this Nullable<CancellationToken> token)
        {
            return token.HasValue && token.Value.IsCancellationRequested;
        }
    }
}
