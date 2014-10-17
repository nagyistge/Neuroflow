using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks
{
    public interface INode
    {
        int Index { get; }
    }
    
    public abstract class Node : INode
    {
        #region Properties

        public int Index { get; internal set; }

        #endregion
    }
}
