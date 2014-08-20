using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks.Neural
{
    [ContractClass(typeof(IUnorderedNeuralVectorFlowProviderContract))]
    public interface IUnorderedNeuralVectorFlowProvider : INeuralVectorFlowProvider
    {
        int ItemCount { get; }

        IEnumerable<NeuralVectorFlow> GetNext(IndexSet indexes);

        void Reinitialize();
    }

    [ContractClassFor(typeof(IUnorderedNeuralVectorFlowProvider))]
    abstract class IUnorderedNeuralVectorFlowProviderContract : IUnorderedNeuralVectorFlowProvider
    {
        int IUnorderedNeuralVectorFlowProvider.ItemCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        IEnumerable<NeuralVectorFlow> IUnorderedNeuralVectorFlowProvider.GetNext(IndexSet indexes)
        {
            IUnorderedNeuralVectorFlowProvider prov = this;
            Contract.Requires(indexes != null);
            Contract.Requires(Contract.ForAll(indexes, (index) => index >= 0 && index < prov.ItemCount));
            Contract.Ensures(Contract.Result<IEnumerable<NeuralVectorFlow>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<NeuralVectorFlow>>(), vf => vf != null));
            return null;
        }

        void IUnorderedNeuralVectorFlowProvider.Reinitialize()
        {
        }

        int IInterfaced.InputInterfaceLength
        {
            get { return 0; }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get { return 0; }
        }
    }
}
