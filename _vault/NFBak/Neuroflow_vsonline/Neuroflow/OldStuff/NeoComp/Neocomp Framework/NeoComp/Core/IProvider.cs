using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Core
{
    public interface IProvider : IDisposable
    {
        bool IsDisposed { get; }
    }
}
