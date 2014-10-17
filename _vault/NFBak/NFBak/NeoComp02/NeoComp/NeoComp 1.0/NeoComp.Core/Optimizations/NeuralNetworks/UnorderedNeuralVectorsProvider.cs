using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.NeuralNetworks
{
    [Serializable]
    [ContractClass(typeof(UnorderedNeuralVectorsProviderContract))]
    public abstract class UnorderedNeuralVectorsProvider : IUnorderedNeuralVectorsProvider
    {
        public UnorderedNeuralVectorsProvider(int? reinitalizationFrequency = null)
        {
            Contract.Requires(reinitalizationFrequency == null || reinitalizationFrequency.Value > 0);
        }

        bool initialized;

        int itemCount;

        public int ItemCount
        {
            get
            {
                EnsureInitialization();

                return itemCount;
            }
        }

        public IEnumerable<NeuralVectors> GetNextVectors(IndexSet indexes)
        {
            EnsureInitialization();

            return DoGetNextVectors(indexes);
        }

        #region Init

        private void EnsureInitialization()
        {
            if (!initialized)
            {
                itemCount = Initialize();
                initialized = true;
            }
        }

        protected abstract int Initialize();

        public void Reinitialize()
        {
            if (!initialized)
            {
                itemCount = Initialize();
                initialized = true;
            }
            else
            {
                itemCount = Initialize();
            }
        }

        #endregion

        #region Get Next Vector Flow

        protected abstract IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes);

        #endregion
    }

    [ContractClassFor(typeof(UnorderedNeuralVectorsProvider))]
    abstract class UnorderedNeuralVectorsProviderContract : UnorderedNeuralVectorsProvider
    {
        protected override IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes)
        {
            Contract.Requires(indexes != null);
            Contract.Requires(Contract.ForAll(indexes, (index) => index >= 0 && index < ItemCount));
            Contract.Ensures(Contract.Result<IEnumerable<NeuralVectors>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<NeuralVectors>>(), vf => vf != null));
            return null;
        }
    }
}
