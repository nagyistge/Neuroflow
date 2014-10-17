using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public class MultilayerArchitecture : Architecture
    {
        public MultilayerArchitecture(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);

            Definition = new NeuralNetworkLayersDefinition();
            Definition.Changed += (sender, e) => OnDefinitionChanged();
        }

        bool generated;

        public NeuralNetworkLayersDefinition Definition { get; private set; }

        private void OnDefinitionChanged()
        {
            if (generated) throw new InvalidOperationException("Definition is read-only.");
        }

        #region Create

        protected override NetworkDefinition<NeuralNode, NeuralConnection> CreateDefinition()
        {
            var def = Build();
            generated = true;
            return def;
        }

        private NetworkDefinition<NeuralNode, NeuralConnection> Build()
        {
            var networkDef = new NetworkDefinition<NeuralNode, NeuralConnection>();
            
            int currentNodeIndex = InputInterfaceLength;
            var nodeLayerInfos = new Dictionary<int, IntRange>();
            nodeLayerInfos.Add(0, IntRange.CreateExclusive(0, InputInterfaceLength));

            int entryCount = Definition.NodeCount;

            int entryIdx = 0; 
            foreach (var nodeEntry in Definition.NodeEntries)
            {
                // Info
                var currentInfo = IntRange.CreateExclusive(currentNodeIndex, currentNodeIndex + nodeEntry.Node.NodeCount);
                nodeLayerInfos.Add(nodeEntry.Index, currentInfo);
                currentNodeIndex += nodeEntry.Node.NodeCount;

                // Upper
                foreach (var conn in Definition.GetUpperConnections(nodeEntry.Index))
                {
                    IntRange prevInfo;
                    if (!nodeLayerInfos.TryGetValue(conn.Index.UpperNodeIndex, out prevInfo))
                    {
                        throw GetArchitectureBuildingErrorEx("Node layer definition at '" + conn.Index.UpperNodeIndex + "' doesn't exists.");
                    }

                    for (int uidx = prevInfo.MinValue; uidx <= prevInfo.MaxValue; uidx++)
                    {
                        for (int lidx = currentInfo.MinValue; lidx <= currentInfo.MaxValue; lidx++)
                        {
                            networkDef.AddConnection(new ConnectionIndex(uidx, lidx), conn.Connection.ConnectionFactory.Create());
                        }
                    }
                }

                // Nodes
                for (int idx = currentInfo.MinValue; idx <= currentInfo.MaxValue; idx++)
                {
                    networkDef.AddNode(idx, nodeEntry.Node.NodeFactory.Create());
                }

                var selfConnDef = nodeEntry.Node.SelfConnectionDefinition;

                if (selfConnDef != null)
                {
                    for (int uidx = currentInfo.MinValue; uidx < currentInfo.MaxValue; uidx++)
                    {
                        for (int lidx = uidx + 1; lidx <= currentInfo.MaxValue; lidx++)
                        {
                            networkDef.AddConnection(new ConnectionIndex(uidx, lidx), selfConnDef.ConnectionFactory.Create());
                        }
                    }
                }

                // Output:
                if (entryIdx == entryCount - 1)
                {
                    if (currentInfo.Size != OutputInterfaceLength)
                    {
                        throw GetArchitectureBuildingErrorEx("Last node layer output size must be same as OutputInterfaceLength.");
                    }

                    for (int uidx = currentInfo.MinValue; uidx <= currentInfo.MaxValue; uidx++)
                    {
                        int lidx = uidx + OutputInterfaceLength;
                        networkDef.AddConnection(new ConnectionIndex(uidx, lidx), new NeuralConnection());
                    }
                }

                entryIdx++;
            }

            return networkDef;
        }

        private static InvalidOperationException GetArchitectureBuildingErrorEx(string msg, Exception ex = null)
        {
            return new InvalidOperationException(
                string.Format("{0} See {1}provided data 'builder' for details.", msg, ex == null ? string.Empty : "inner exception "), ex);
        }

        #endregion
    }
}
