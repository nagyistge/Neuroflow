using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Neuroflow.Core.Threading
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "syncObj")]
    public abstract class SynchronizedObject : ISynchronized
    {
        protected SynchronizedObject()
            : this(null)
        {
        }

        protected SynchronizedObject(SyncContext syncRoot)
        {
            SyncRoot = syncRoot == null ? new SyncContext() : syncRoot;
        }

        [DataMember(Name = "syncRoot")]
        public SyncContext SyncRoot { get; private set; }
    }
}
