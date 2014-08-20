using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.ComputationAPI
{
    public abstract class ComputationHandle : MarshalByRefObject
    {
        public ComputationHandle(IntPtr ptr)
        {
            this.ptr = ptr;
        }
        
        protected IntPtr ptr;
        
        unsafe public abstract void Run();
    }
}
