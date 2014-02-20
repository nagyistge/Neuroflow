using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public sealed class ResetHandler : IReset
    {
        public ResetHandler(object obj, bool synchronizedReset = true)
        {
            Contract.Requires(obj != null);

            var resetable = obj as IResetable;
            if (resetable != null)
            {
                reset = resetable.GetReset();
            }
            else
            {
                reset = obj as IReset;
            }
            
            if (synchronizedReset)
            {
                sync = obj as ISynchronized;
            }
        }
        
        IReset reset;

        ISynchronized sync;

        public bool IsResetable
        {
            get { return reset != null; }
        }

        public void Reset()
        {
            if (reset == null) return;

            if (sync != null)
            {
                lock (sync.SyncRoot) reset.Reset();
            }
            else
            {
                reset.Reset();
            }
        }
    }
}
