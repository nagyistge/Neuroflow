﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural
{
    [Serializable]
    [ContractClass(typeof(UnorderedNeuralVectorFlowProviderContract))]
    public abstract class UnorderedNeuralVectorFlowProvider : IUnorderedNeuralVectorFlowProvider
    {
        protected struct InitializationData : IInterfaced
        {
            public InitializationData(int itemCount, int inputInterfaceLength, int outputInterfaceLength)
            {
                Contract.Requires(itemCount > 0);
                Contract.Requires(inputInterfaceLength > 0);
                Contract.Requires(outputInterfaceLength > 0);

                this.itemCount = itemCount;
                this.inputInterfaceLength = inputInterfaceLength;
                this.outputInterfaceLength = outputInterfaceLength;
            }

            int itemCount;

            public int ItemCount
            {
                get { return itemCount; }
            }

            int inputInterfaceLength;

            public int InputInterfaceLength
            {
                get { return inputInterfaceLength; }
            }

            int outputInterfaceLength;

            public int OutputInterfaceLength
            {
                get { return outputInterfaceLength; }
            }
        }

        InitializationData? initData;

        public int ItemCount
        {
            get
            {
                EnsureInitialization();

                return initData.Value.ItemCount;
            }
        }

        public int InputInterfaceLength
        {
            get
            {
                EnsureInitialization();

                return initData.Value.InputInterfaceLength;
            }
        }

        public int OutputInterfaceLength
        {
            get
            {
                EnsureInitialization();

                return initData.Value.OutputInterfaceLength;
            }
        }

        public IEnumerable<NeuralVectorFlow> GetNext(IndexSet indexes)
        {
            EnsureInitialization();

            return DoGetNext(indexes);
        }

        #region Init

        private void EnsureInitialization()
        {
            if (initData == null)
            {
                initData = Initialize();
            }
        }

        protected abstract InitializationData Initialize();

        public void Reinitialize()
        {
            initData = Initialize();
        }

        #endregion

        #region Get Next Vector Flow

        protected abstract IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes);

        #endregion
    }

    [ContractClassFor(typeof(UnorderedNeuralVectorFlowProvider))]
    abstract class UnorderedNeuralVectorFlowProviderContract : UnorderedNeuralVectorFlowProvider
    {
        protected override IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes)
        {
            Contract.Requires(indexes != null);
            Contract.Requires(Contract.ForAll(indexes, (index) => index >= 0 && index < ItemCount));
            Contract.Ensures(Contract.Result<IEnumerable<NeuralVectorFlow>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<NeuralVectorFlow>>(), vf => vf != null));
            return null;
        }
    }
}