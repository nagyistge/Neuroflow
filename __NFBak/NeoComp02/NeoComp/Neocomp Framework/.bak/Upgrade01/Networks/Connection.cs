using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks
{
    public interface IConnection
    {
        ConnectionIndex Index { get; }
    }
    
    public abstract class Connection : IConnection
    {
        #region Properties

        public ConnectionIndex Index { get; internal set; }

        #endregion
    }
}
