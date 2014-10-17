using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    public sealed class ConnectionInfo<TConnection> : ReadOnlyCollection<ConnectionCollection<TConnection>>
        where TConnection : IConnection
    {
        internal ConnectionInfo(int size)
            : base(Enumerable.Range(0, size).Select(idx => new ConnectionCollection<TConnection>()).ToList())
        {
            Contract.Requires(size >= 0);
        }
    }
}
