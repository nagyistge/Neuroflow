using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    public class ConnectionLayerDefinition<T>
        where T : struct
    {
        public ConnectionLayerDefinition(IFactory<ComputationalConnection<T>> connectionFactory, bool recurrent = false)
        {
            Contract.Requires(connectionFactory != null);
            
            ConnectionFactory = connectionFactory;
            Recurrent = recurrent;
        }

        public IFactory<ComputationalConnection<T>> ConnectionFactory { get; private set; }

        public bool Recurrent { get; private set; }
    }
}
