using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Neuroflow.Core
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow)]
    public sealed class SyncContext
    {
    }
}
