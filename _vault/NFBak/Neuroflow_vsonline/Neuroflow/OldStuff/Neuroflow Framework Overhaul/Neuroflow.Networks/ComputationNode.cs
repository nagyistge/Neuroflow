using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Neuroflow.Networks
{
    public abstract class ComputationNode<T> : ComputationNetworkItem<T> where T : struct
    {
        public virtual bool SupportsNoUpperConnections
        {
            get { return false; }
        }

        public virtual bool SupportsNoLowerConnections
        {
            get { return false; }
        }

        public virtual bool IsOpeartionNode
        {
            get { return true; }
        }

        public int TimeFrame { get; internal set; }

        internal ComputationConnection<T>[] upperConnectionArray;

        [Browsable(false)]
        public ReadOnlyCollection<ComputationConnection<T>> UpperConnections
        {
            get
            {
                CheckIsInitialized();
                return Array.AsReadOnly(upperConnectionArray);
            }
        }

        internal ComputationConnection<T>[] lowerConnectionArray;

        [Browsable(false)]
        public ReadOnlyCollection<ComputationConnection<T>> LowerConnections
        {
            get
            {
                CheckIsInitialized();
                return Array.AsReadOnly(lowerConnectionArray);
            }
        }

        int? index;

        public int Index
        {
            get
            {
                CheckIsInitialized();
                return index.Value;
            }
        }

        internal void Initialize(int index, ValueSpace<T> valueSpace, IEnumerable<ComputationConnection<T>> upperConns, IEnumerable<ComputationConnection<T>> lowerConns)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(valueSpace != null);
            Contract.Requires(upperConns != null);
            Contract.Requires(lowerConns != null);
            
            RegisterValueSpace(valueSpace);

            this.index = index;
            upperConnectionArray = upperConns.ToArray();
            lowerConnectionArray = lowerConns.ToArray();

            Initialize();
        }

        protected virtual void Initialize() { }

        internal ComputationBlock GetComputationBlock()
        {
            var block = CreateComputationBlock();
            if (block == null) throw new InvalidOperationException("Computation node's CreateComputationBlock method has returned a null value.");
            return block;
        }

        protected abstract ComputationBlock CreateComputationBlock();
    }
}
