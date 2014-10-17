using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public abstract class Architecture : IInterfaced
    {
        protected Architecture(int inputInterfaceLength, int outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0); 
            
            UID = Guid.NewGuid();
            InputInterfaceLength = inputInterfaceLength;
            OutputInterfaceLength = outputInterfaceLength;
        }
        
        public Guid UID { get; private set; }

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; }

        public NeuralNetwork CreateNetwork()
        {
            var def = CreateDefinition();
            if (def == null) throw new InvalidOperationException("Cannot create network, generated definition is null.");

            if (def.ConnectionCount == 0 || def.NodeCount == 0) throw new InvalidOperationException("Created architecture definition is empty.");

            var nn = new NeuralNetwork(UID, InputInterfaceLength, OutputInterfaceLength);
            nn.Build(def);
            return nn;
        }

        protected abstract NetworkDefinition<NeuralNode, NeuralConnection> CreateDefinition();
    }
}
