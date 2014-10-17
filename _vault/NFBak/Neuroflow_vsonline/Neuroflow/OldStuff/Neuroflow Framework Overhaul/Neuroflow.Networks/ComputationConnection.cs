using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks
{
    public abstract class ComputationConnection<T> : ComputationNetworkItem<T> where T : struct
    {
        internal void InitializeWithInputIndex(ConnectionIndex index, ValueSpace<T> valueSpace, int? inputValueIndex)
        {
            Contract.Requires(valueSpace != null);
            Contract.Requires(inputValueIndex >= 0);
            Contract.Requires(index.UpperNodeIndex >= 0);
            Contract.Requires(index.LowerNodeIndex >= 0);

            this.index = index;
            RegisterValueSpace(valueSpace);
            if (inputValueIndex != null)
            {
                inputValue = new ValueReference<T>(valueSpace, inputValueIndex.Value);
            }
            else
            {
                inputValue = new ValueReference<T>(valueSpace);
            }

            Initialize();
        }

        ConnectionIndex? index;

        public ConnectionIndex Index
        {
            get
            {
                CheckIsInitialized();
                return index.Value;
            }
        }

        internal ValueReference<T> inputValue;

        public ValueReference<T> InputValue
        {
            get
            {
                CheckIsInitialized();
                return inputValue;
            }
        }

        protected virtual void Initialize() { }

        protected internal virtual string CreateComputationExpression()
        {
            return InputValue.Ref;
        }

        internal void AddResetCode(ComputationBlock block)
        {
            block.Add(inputValue.Ref + "=0.0");
        }
    }
}
