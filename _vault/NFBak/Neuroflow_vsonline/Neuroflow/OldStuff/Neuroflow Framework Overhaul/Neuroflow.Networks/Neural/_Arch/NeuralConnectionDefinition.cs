using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public class NeuralConnectionDefinition
    {
        public NeuralConnectionDefinition(IFactory<NeuralConnection> connectionFactory, bool isRecurrent)
        {
            Contract.Requires(connectionFactory != null);

            ConnectionFactory = connectionFactory;
            IsRecurrent = isRecurrent;
        }
        
        public IFactory<NeuralConnection> ConnectionFactory { get; private set; }

        public bool IsRecurrent { get; private set; }
    }
}
