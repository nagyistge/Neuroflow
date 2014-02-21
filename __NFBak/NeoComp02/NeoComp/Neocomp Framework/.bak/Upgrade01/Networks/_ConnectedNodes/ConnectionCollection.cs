using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    public sealed class ConnectionCollection<TConnection> : ReadOnlyCollection<TConnection>
        where TConnection : IConnection
    {
        internal ConnectionCollection()
            : base(new List<TConnection>())
        {
        }

        internal void AddInternal(TConnection connection)
        {
            Contract.Requires(connection != null);

            Items.Add(connection);
        }
    }
}
