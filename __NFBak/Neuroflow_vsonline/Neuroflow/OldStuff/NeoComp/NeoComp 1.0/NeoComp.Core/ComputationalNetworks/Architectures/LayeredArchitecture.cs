using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp;
using NeoComp.Networks;

namespace NeoComp.ComputationalNetworks.Architectures
{
    [Serializable]
    public abstract class LayeredArchitecture<T> : Architecture<T>
        where T : struct
    {
        #region Network Class

        class Network : Network<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>
        {
            internal Network(LayeredArchitectureFactory<T> factory)
                : base(factory)
            {
                Contract.Requires(factory != null);
            }

            new internal int MaxEntryIndex
            {
                get { return base.MaxEntryIndex; }
            }
        } 

        #endregion

        #region Constructors

        protected LayeredArchitecture(ILayeredArchitectureBuilder<T> builder)
            : base(builder.InputInterfaceLength, builder.OutputInterfaceLength)
        {
            Contract.Requires(builder != null);
            this.builder = builder;
        }

        #endregion

        #region Fields and Properties

        ILayeredArchitectureBuilder<T> builder;

        #endregion

        #region Build

        protected override void Build(ComputationalNetworkFactory<T> factory)
        {
            var archFactory = new LayeredArchitectureFactory<T>();
            Network archNetwork = null;

            try
            {
                builder.Build(archFactory);
                archNetwork = new Network(archFactory);
            }
            catch (Exception ex)
            {
                throw GetArchitectureBuildingErrorEx("Architecture building error.", ex);
            }

            if (archNetwork.MaxEntryIndex < 1 || archNetwork.EntryArray.Length == 0) throw GetArchitectureBuildingErrorEx("Built architecture is incomplete.");

            Build(factory, archNetwork);
        }

        private void Build(ComputationalNetworkFactory<T> factory, Network archNetwork)
        {
            int currentNodeIndex = InputInterfaceLength;
            var nodeLayerInfos = new Dictionary<int, IntRange>();
            nodeLayerInfos.Add(0, IntRange.CreateExclusive(0, InputInterfaceLength));

            int entryCount = archNetwork.EntryArray.Length;
            for (int entryIdx = 0; entryIdx < entryCount; entryIdx++)
            {
                var entry = archNetwork.EntryArray[entryIdx];
                
                // Info
                var currentInfo = IntRange.CreateExclusive(currentNodeIndex, currentNodeIndex + entry.NodeEntry.Node.NodeCount);
                nodeLayerInfos.Add(entry.NodeEntry.Index, currentInfo);
                currentNodeIndex += entry.NodeEntry.Node.NodeCount;

                // Upper
                foreach (var conn in entry.UpperConnectionEntryArray)
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
                            factory.AddConnectionFactory(new ConnectionIndex(uidx, lidx), conn.Connection.ConnectionFactory);
                        }
                    }
                }

                // Nodes
                for (int idx = currentInfo.MinValue; idx <= currentInfo.MaxValue; idx++)
                {
                    factory.AddNodeFactory(idx, entry.NodeEntry.Node.NodeFactory);
                }

                var wireLayerDef =
                    entry.NodeEntry.Node is OperationNodeLayerDefinition<T> ?
                    ((OperationNodeLayerDefinition<T>)entry.NodeEntry.Node).WireConnectionLayerDefinition :
                    null;

                if (wireLayerDef != null)
                {
                    for (int uidx = currentInfo.MinValue; uidx < currentInfo.MaxValue; uidx++)
                    {
                        for (int lidx = uidx + 1; lidx <= currentInfo.MaxValue; lidx++)
                        {
                            factory.AddConnectionFactory(new ConnectionIndex(uidx, lidx), wireLayerDef.ConnectionFactory);
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
                        factory.AddConnectionFactory(new ConnectionIndex(uidx, lidx), OutputInterfaceConnectionFactory);
                    }
                }
            }
        }

        private static InvalidOperationException GetArchitectureBuildingErrorEx(string msg, Exception ex = null)
        {
            return new InvalidOperationException(
                string.Format("{0} See {1}provided data 'builder' for details.", msg, ex == null ? string.Empty : "inner exception "), ex);
        }

        #endregion
    }
}
