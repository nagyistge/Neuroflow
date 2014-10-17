using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks
{
    public abstract class ComputationNetworkItem<T> where T : struct
    {
        internal ValueSpace<T> valueSpace;

        protected ValueSpace<T> ValueSpace
        {
            get
            {
                CheckIsInitialized();
                return valueSpace;
            }
            private set { valueSpace = value; }
        }

        public bool IsInitialized
        {
            get { return valueSpace != null; }
        }

        protected void RegisterValueSpace(ValueSpace<T> valueSpace)
        {
            Contract.Requires(valueSpace != null);

            if (this.valueSpace == null)
            {
                this.valueSpace = valueSpace;
            }
            else
            {
                throw new InvalidOperationException("Network item's Value Space already initialized.");
            }
        }

        protected void CheckIsInitialized()
        {
            if (!IsInitialized) throw new InvalidOperationException("Network item is not initialized.");
        }
    }
}
