using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Neuroflow.Core
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow)]
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

        [DataMember(Name = "SyncRoot")]
        public SyncContext SyncRoot { get; private set; }
    }
}
