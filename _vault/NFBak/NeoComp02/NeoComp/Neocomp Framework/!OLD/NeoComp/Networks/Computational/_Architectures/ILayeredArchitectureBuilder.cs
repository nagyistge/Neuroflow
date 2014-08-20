using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational
{
    [ContractClass(typeof(ILayeredArchitectureBuilderContract<>))]
    public interface ILayeredArchitectureBuilder<T> : IInterfaced
    {
        void Build(LayeredArchitectureFactory<T> factory);
    }

    [ContractClassFor(typeof(ILayeredArchitectureBuilder<>))]
    class ILayeredArchitectureBuilderContract<T> : ILayeredArchitectureBuilder<T>
    {
        void ILayeredArchitectureBuilder<T>.Build(LayeredArchitectureFactory<T> factory)
        {
            Contract.Requires(factory != null);
        }

        int IInterfaced.InputInterfaceLength
        {
            get { throw new NotImplementedException(); }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get { throw new NotImplementedException(); }
        }
    }
}
