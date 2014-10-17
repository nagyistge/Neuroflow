using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational
{
    public abstract class ComputationalNetworkFactory<T> : 
        NetworkFactory<ComputationalNode<T>, ComputationalConnection<T>>, 
        INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>,
        IInterfaced
    {
        #region Constructors

        protected ComputationalNetworkFactory(int inputInterfaceLength, int outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);

            InputInterfaceLength = inputInterfaceLength;
            OutputInterfaceLength = outputInterfaceLength;
        }

        protected ComputationalNetworkFactory(ComputationalNetwork<T> network)
            : base(network)
        {
            Contract.Requires(network != null);

            InputInterfaceLength = network.InputInterface.Length;
            OutputInterfaceLength = network.OutputInterface.Length;
        } 

        #endregion

        #region Properties

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; } 

        #endregion

        #region Rules

        protected virtual bool IsValidNodeEntry(NodeEntry<ComputationalNode<T>> entry)
        {
            return true;
        }

        protected virtual bool IsValidConnectionEntry(NodeEntry<ComputationalNode<T>> parent, ConnectionEntry<ComputationalConnection<T>> entry)
        {
            return true;
        }

        protected virtual bool IsValidNetworkEntry(NetworkEntry<ComputationalNode<T>, ComputationalConnection<T>> entry)
        {
            return entry.LowerConnectionEntryArray.Length > 0 && entry.UpperConnectionEntryArray.Length > 0; // Has Output And Input.
        }

        protected virtual bool IsValidNodeFactoryEntry(NodeFactoryEntry<ComputationalNode<T>> entry)
        {
            return entry.Index >= InputInterfaceLength && entry.Index <= MaxEntryIndex - OutputInterfaceLength; // Node is not on Input or Output Interface pins.
        }

        protected virtual bool IsValidConnectionFactoryEntry(NodeFactoryEntry<ComputationalNode<T>> parent, ConnectionFactoryEntry<ComputationalConnection<T>> entry)
        {
            return entry.Index.LowerNodeIndex >= InputInterfaceLength && entry.Index.UpperNodeIndex <= MaxEntryIndex - OutputInterfaceLength; // I/O area are not recurrent.
        }

        protected virtual bool IsValidNetworkFactoryEntry(NetworkFactoryEntry<ComputationalNode<T>, ComputationalConnection<T>> entry)
        {
            return true;
        } 

        #endregion

        #region Rules Impl

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidNodeEntry(NodeEntry<ComputationalNode<T>> entry)
        {
            return IsValidNodeEntry(entry);
        }

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidConnectionEntry(NodeEntry<ComputationalNode<T>> parent, ConnectionEntry<ComputationalConnection<T>> entry)
        {
            return IsValidConnectionEntry(parent, entry);
        }

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidNetworkEntry(NetworkEntry<ComputationalNode<T>, ComputationalConnection<T>> entry)
        {
            return IsValidNetworkEntry(entry);
        }

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidNodeFactoryEntry(NodeFactoryEntry<ComputationalNode<T>> entry)
        {
            return IsValidNodeFactoryEntry(entry);
        }

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidConnectionFactoryEntry(NodeFactoryEntry<ComputationalNode<T>> parent, ConnectionFactoryEntry<ComputationalConnection<T>> entry)
        {
            return IsValidConnectionFactoryEntry(parent, entry);
        }

        bool INetworkBuildingRules<ComputationalNode<T>, ComputationalConnection<T>>.IsValidNetworkFactoryEntry(NetworkFactoryEntry<ComputationalNode<T>, ComputationalConnection<T>> entry)
        {
            return IsValidNetworkFactoryEntry(entry);
        } 

        #endregion
    }
}
