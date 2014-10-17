using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks
{
    public interface IConnectedNode
    {
        IEnumerable<IConnection> UpperConnections { get; }

        IEnumerable<IConnection> LowerConnections { get; }

        INode Node { get; }
    }
}
