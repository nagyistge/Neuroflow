using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace Neuroflow.Activities
{
    internal static class ActivityHelpers
    {
        internal static bool IsNull(this ActivityDelegate ad)
        {
            if (ad == null) return true;
            return ad.Handler == null;
        }
    }
}
