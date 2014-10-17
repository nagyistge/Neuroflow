using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    [ContractClass(typeof(INetworkBuildingRulesContract<,>))]
    public interface INetworkBuildingRules<TNode, TConnection>
    {
        bool IsValidNodeEntry(NodeEntry<TNode> entry);

        bool IsValidConnectionEntry(NodeEntry<TNode> parent, ConnectionEntry<TConnection> entry);

        bool IsValidNetworkEntry(NetworkEntry<TNode, TConnection> entry);

        bool IsValidNodeFactoryEntry(NodeFactoryEntry<TNode> entry);

        bool IsValidConnectionFactoryEntry(NodeFactoryEntry<TNode> parent, ConnectionFactoryEntry<TConnection> entry);

        bool IsValidNetworkFactoryEntry(NetworkFactoryEntry<TNode, TConnection> entry);
    }

    [ContractClassFor(typeof(INetworkBuildingRules<,>))]
    class INetworkBuildingRulesContract<TNode, TConnection> : INetworkBuildingRules<TNode, TConnection>
    {
        bool INetworkBuildingRules<TNode, TConnection>.IsValidNodeEntry(NodeEntry<TNode> entry)
        {
            Contract.Requires(entry != null);
            
            return false;
        }

        bool INetworkBuildingRules<TNode, TConnection>.IsValidConnectionEntry(NodeEntry<TNode> parent, ConnectionEntry<TConnection> entry)
        {
            Contract.Requires(parent != null);
            Contract.Requires(entry != null);
            
            return false;
        }

        bool INetworkBuildingRules<TNode, TConnection>.IsValidNetworkEntry(NetworkEntry<TNode, TConnection> entry)
        {
            Contract.Requires(entry != null); 
            
            return false;
        }

        bool INetworkBuildingRules<TNode, TConnection>.IsValidNodeFactoryEntry(NodeFactoryEntry<TNode> entry)
        {
            Contract.Requires(entry != null); 
            
            return false;
        }

        bool INetworkBuildingRules<TNode, TConnection>.IsValidConnectionFactoryEntry(NodeFactoryEntry<TNode> parent, ConnectionFactoryEntry<TConnection> entry)
        {
            Contract.Requires(parent != null);
            Contract.Requires(entry != null);
            
            return false;
        }

        bool INetworkBuildingRules<TNode, TConnection>.IsValidNetworkFactoryEntry(NetworkFactoryEntry<TNode, TConnection> entry)
        {
            Contract.Requires(entry != null); 
            
            return false;
        }
    }

}
