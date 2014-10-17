using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [ContractClass(typeof(ArchitectureContract<>))]
    public abstract class Architecture<T> : IInterfaced
        where T : struct
    {
        #region Constructors

        protected Architecture(int inputInterfaceLength, int outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);

            InputInterfaceLength = inputInterfaceLength;
            OutputInterfaceLength = outputInterfaceLength;
        }

        #endregion

        #region Properties

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; }

        protected abstract IFactory<ComputationalConnection<T>> OutputInterfaceConnectionFactory { get; }

        #endregion

        #region Create

        protected ComputationalNetworkFactory<T> CreateFactory()
        {
            var factory = CreateFactoryInstance();
            Build(factory);
            return factory;
        }

        protected ComputationalNetwork<T> CreateNetwork()
        {
            return CreateNetworkInstance(CreateFactory());
        }

        protected abstract void Build(ComputationalNetworkFactory<T> factory);

        protected abstract ComputationalNetworkFactory<T> CreateFactoryInstance();

        protected abstract ComputationalNetwork<T> CreateNetworkInstance(ComputationalNetworkFactory<T> factory);

        #endregion
    }

    [ContractClassFor(typeof(Architecture<>))]
    abstract class ArchitectureContract<T> : Architecture<T>
        where T : struct
    {
        protected ArchitectureContract()
            : base(0, 0)
        {
        }

        protected override IFactory<ComputationalConnection<T>> OutputInterfaceConnectionFactory
        {
            get 
            {
                Contract.Ensures(Contract.Result<IFactory<ComputationalConnection<T>>>() != null);
                return null;
            }
        }

        protected override ComputationalNetworkFactory<T> CreateFactoryInstance()
        {
            Contract.Ensures(Contract.Result<ComputationalNetworkFactory<T>>() != null);
            Contract.Ensures(Contract.Result<ComputationalNetworkFactory<T>>().InputInterfaceLength == InputInterfaceLength);
            Contract.Ensures(Contract.Result<ComputationalNetworkFactory<T>>().OutputInterfaceLength == OutputInterfaceLength);
            return null;
        }

        protected override ComputationalNetwork<T> CreateNetworkInstance(ComputationalNetworkFactory<T> factory)
        {
            //Contract.Requires(factory != null);
            //Contract.Requires(factory.InputInterfaceLength == InputInterfaceLength);
            //Contract.Requires(factory.OutputInterfaceLength == OutputInterfaceLength);
            Contract.Ensures(Contract.Result<ComputationalNetwork<T>>() != null);
            Contract.Ensures(Contract.Result<ComputationalNetwork<T>>().InputInterface.Length == InputInterfaceLength);
            Contract.Ensures(Contract.Result<ComputationalNetwork<T>>().OutputInterface.Length == OutputInterfaceLength);
            return null;
        }
    }
}
