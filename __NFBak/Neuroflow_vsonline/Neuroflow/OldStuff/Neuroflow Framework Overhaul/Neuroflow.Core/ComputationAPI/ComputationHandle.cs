using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.ComputationAPI
{
    public abstract class ComputationHandle
    {
        public ComputationHandle(IntPtr ptr, object context = null)
        {
            this.ptr = ptr;
            this.context = context;
        }
        
        protected IntPtr ptr;

        protected object context;
        
        unsafe public abstract void Run();
    }
}
