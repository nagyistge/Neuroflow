using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NeoComp.Core
{
    internal class SyncObjectGuard : IDisposable
    {
        internal SyncObjectGuard(object obj)
        {
            sync = obj as ISynchronized;
            if (sync != null) Monitor.Enter(sync.SyncRoot, ref taken);
        }

        ISynchronized sync;

        bool taken;

        public void Dispose()
        {
            if (taken) Monitor.Exit(sync.SyncRoot);
        }
    }
}
