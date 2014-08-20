using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using NeoComp.Computations;

namespace NeoComp.Networks
{
    public sealed class NetworkInterface<TConnection, T> : ComputationalInterface<T>
        where TConnection : ComputationalConnection<T>
    {
        #region classes

        internal class ConnectionCollection : ReadOnlyCollection<TConnection>
        {
            internal ConnectionCollection()
                : base(new List<TConnection>())
            {
            }

            internal void Clear()
            {
                Items.Clear();
            }

            internal void Add(TConnection connection)
            {
                Contract.Requires(connection != null);

                Items.Add(connection);
            }
        }

        public class ConnectionInfo
        {
            internal ConnectionInfo(NetworkInterface<TConnection, T> owner, int length)
            {
                Contract.Requires(owner != null);
                Contract.Requires(length > 0);

                this.owner = owner;
                collections = Enumerable.Range(0, length).Select(i => new ConnectionCollection()).ToArray();
            }

            NetworkInterface<TConnection, T> owner;

            ConnectionCollection[] collections;

            public int Count
            {
                get { return collections.Length; }
            }

            public ReadOnlyCollection<TConnection> this[int index]
            {
                get
                {
                    Contract.Requires(index >= 0 && index < Count);
                    owner.OwnerNetwork.EnsureContext();
                    return collections[index];
                }
            }

            internal ConnectionCollection GetCollection(int index)
            {
                Contract.Requires(index >= 0 && index < Count);

                return collections[index];
            }

            internal void Clear()
            {
                foreach (var coll in collections) coll.Clear();
            }
        }

        #endregion

        #region Constructor

        internal NetworkInterface(ComputationalNetwork<TConnection, T> ownerNetwork, int length)
            : base(length, ownerNetwork.SyncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(ownerNetwork != null);

            OwnerNetwork = ownerNetwork;
            Connections = new NetworkInterface<TConnection, T>.ConnectionInfo(this, length);
        } 

        #endregion

        #region Properties

        public ComputationalNetwork<TConnection, T> OwnerNetwork { get; private set; }

        public ConnectionInfo Connections { get; private set; }

        #endregion

        #region Access

        internal ComputationalValue<T> GetDataValue(int index)
        {
            Contract.Requires(index >= 0 && index < Length);

            return Values[index];
        }

        #endregion
    }
}
