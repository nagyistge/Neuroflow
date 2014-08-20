using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Threading;

namespace NeoComp.Features
{
    internal sealed class FeaturedObjectAccess : IDisposable
    {
        public FeaturedObjectAccess(object obj, string featureID = null)
        {
            Contract.Requires(obj != null);

            this.obj = obj;
            this.sync = obj as ISynchronized;
            this.init = obj as IInitializableFeaturedObject;
            if (this.sync != null) Monitor.Enter(this.sync.SyncRoot, ref lockTaken);
            if (this.init != null) this.init.Initialize(featureID);
        }

        object obj;

        ISynchronized sync;

        IInitializableFeaturedObject init;

        bool lockTaken;

        public void Dispose()
        {
            if (this.init != null) this.init.Uninitialize();
            if (this.sync != null) Monitor.Exit(this.sync.SyncRoot);
        }
    }
}
