using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public class NeuralNetworkLayersDefinition : NetworkDefinition<NeuralLayerDefinition, NeuralConnectionDefinition>
    {
        internal event EventHandler Changed;

        private void RaiseChanged()
        {
            var h = Changed;
            if (h != null) h(this, EventArgs.Empty);
        }

        public override void AddConnection(ConnectionIndex index, NeuralConnectionDefinition connection)
        {
            RaiseChanged();
            base.AddConnection(index, connection);
        }

        public override void AddNode(int index, NeuralLayerDefinition node)
        {
            RaiseChanged(); 
            base.AddNode(index, node);
        }

        public override void RemoveConnection(ConnectionIndex index)
        {
            RaiseChanged(); 
            base.RemoveConnection(index);
        }

        public override void RemoveNode(int index)
        {
            RaiseChanged(); 
            base.RemoveNode(index);
        }
    }
}
