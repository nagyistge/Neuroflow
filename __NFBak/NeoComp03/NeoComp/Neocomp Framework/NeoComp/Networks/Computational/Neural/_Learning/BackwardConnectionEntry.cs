using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class BackwardConnectionEntry
    {
        internal BackwardConnectionEntry(ConnectionIndex index, IBackwardConnection connection)
        {
            Contract.Requires(connection != null);

            Index = index;
            Connection = connection;
        }
        
        public ConnectionIndex Index { get; private set; }

        public IBackwardConnection Connection { get; private set; }
    }
}
