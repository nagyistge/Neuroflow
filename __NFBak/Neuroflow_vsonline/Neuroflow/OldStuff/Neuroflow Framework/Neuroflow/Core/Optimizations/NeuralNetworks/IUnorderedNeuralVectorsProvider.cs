using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Optimizations.NeuralNetworks
{
    [ContractClass(typeof(IUnorderedNeuralVectorsProviderContract))]
    public interface IUnorderedNeuralVectorsProvider : INeuralVectorsProvider
    {
        int ItemCount { get; }

        IEnumerable<NeuralVectors> GetNextVectors(IndexSet indexes);

        void Reinitialize();
    }

    [ContractClassFor(typeof(IUnorderedNeuralVectorsProvider))]
    class IUnorderedNeuralVectorsProviderContract : IUnorderedNeuralVectorsProvider
    {
        int IUnorderedNeuralVectorsProvider.ItemCount
        {
            get 
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        IEnumerable<NeuralVectors> IUnorderedNeuralVectorsProvider.GetNextVectors(IndexSet indexes)
        {
            IUnorderedNeuralVectorsProvider prov = this;
            Contract.Requires(indexes != null);
            Contract.Requires(Contract.ForAll(indexes, (index) => index >= 0 && index < prov.ItemCount));
            Contract.Ensures(Contract.Result<IEnumerable<NeuralVectors>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<NeuralVectors>>(), vf => vf != null));
            return null;
        }

        void IUnorderedNeuralVectorsProvider.Reinitialize()
        {
        }

        int ComputationalNetworks.IInterfaced.InputInterfaceLength
        {
            get { return 0; }
        }

        int ComputationalNetworks.IInterfaced.OutputInterfaceLength
        {
            get { return 0; }
        }
    }
}
