﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    public sealed class ActiveConnectionFactoryEntry<T> : ConnectionFactoryEntry<T>
    {
        internal ActiveConnectionFactoryEntry(ConnectionEntry<T> activeConnectionEntry)
            : base(activeConnectionEntry.Index, new ClonerFactory<T>(activeConnectionEntry.Connection))
        {
            Contract.Requires(activeConnectionEntry != null);

            Connection = activeConnectionEntry.Connection;
        }

        public T Connection { get; private set; }
    }
}
