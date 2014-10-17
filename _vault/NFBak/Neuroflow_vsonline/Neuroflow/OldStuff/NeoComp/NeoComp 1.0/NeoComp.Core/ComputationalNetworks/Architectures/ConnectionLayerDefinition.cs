using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Diagnostics.Contracts;

namespace NeoComp.ComputationalNetworks.Architectures
{
    [Serializable]
    public class ConnectionLayerDefinition<T>
        where T : struct
    {
        public ConnectionLayerDefinition(IFactory<ComputationConnection<T>> connectionFactory, bool recurrent = false)
        {
            Contract.Requires(connectionFactory != null);
            
            ConnectionFactory = connectionFactory;
            Recurrent = recurrent;
        }

        public IFactory<ComputationConnection<T>> ConnectionFactory { get; private set; }

        public bool Recurrent { get; private set; }
    }
}
