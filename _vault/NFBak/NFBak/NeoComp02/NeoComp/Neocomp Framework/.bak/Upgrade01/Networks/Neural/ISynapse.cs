using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Networks.Neural
{
    public interface ISynapse : IConnection, IAdjustableItem
    {
        double Weight { get; set; }

        double Input { get; }

        double Output { get; }
    }
}
