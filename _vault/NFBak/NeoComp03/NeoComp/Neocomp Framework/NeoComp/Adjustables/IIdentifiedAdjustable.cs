using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Adjustables
{
    public interface IIdentifiedAdjustable : IAdjustable
    {
        Guid UID { get; }
    }
}
