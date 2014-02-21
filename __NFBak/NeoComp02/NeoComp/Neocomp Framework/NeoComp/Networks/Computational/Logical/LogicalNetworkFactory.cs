using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace NeoComp.Networks.Computational.Logical
{
    public class LogicalNetworkFactory : ComputationalNetworkFactory<bool>
    {
        #region Constructors

        public LogicalNetworkFactory(int inputInterfaceLength, int outputInterfaceLength, LogicGateTypes logicGateRestrictions = null)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);

            this.logicGateRestrictions = logicGateRestrictions;
        }

        public LogicalNetworkFactory(LogicalNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);

            logicGateRestrictions = network.LogicGateRestrictions;
        } 

        #endregion

        #region Properties

        LogicGateTypes logicGateRestrictions;

        public LogicGateTypes LogicGateRestrictions
        {
            get
            {
                lock (SyncRoot) return logicGateRestrictions;
            }
            set
            {
                lock (SyncRoot) logicGateRestrictions = value;
            }
        }

        #endregion

        #region Override

        protected internal override bool OverrideNetworkEntry(ModifyableNetworkEntry<ComputationalNode<bool>, ComputationalConnection<bool>> networkEntry, HashSet<int> occupiedNodeIndexes)
        {
            if (base.OverrideNetworkEntry(networkEntry, occupiedNodeIndexes))
            {
                var gate = networkEntry.Node as LogicGate;
                if (gate != null) return OverrideGateEntry(networkEntry);

                return true;
            }
            return false;
        }

        private bool OverrideGateEntry(ModifyableNetworkEntry<ComputationalNode<bool>, ComputationalConnection<bool>> networkEntry)
        {
            var gate = (LogicGate)networkEntry.Node;

            // Logic Gates must have atleast 2 input conns except NOR and NAND:
            if (networkEntry.UpperConnectionEntries.Count == 1
                && !(gate.Operation == LogicalOperation.NAND || gate.Operation == LogicalOperation.NOR))
            {
                return false;
            }

            // Check restrictions:
            if (logicGateRestrictions != null)
            {
                // We should check uppers.
                var connectionEntries = networkEntry.UpperConnectionEntries;

                if (logicGateRestrictions.Operations.Contains(gate.Operation))
                {
                    // Apply if we 2 entry or more.
                    if (connectionEntries.Count >= 2)
                    {
                        var gateType = new LogicGateType(gate.Operation, connectionEntries.Count);
                        bool contains = logicGateRestrictions.Contains(gateType);

                        // Remove entries from end till unresticted gate found.
                        while (connectionEntries.Count > 0 && !contains)
                        {
                            int idx = connectionEntries.Count - 1;
                            connectionEntries.RemoveAt(idx);
                            if (connectionEntries.Count > 0 && (connectionEntries.Count >= 2 || gate.Operation == LogicalOperation.NOR || gate.Operation == LogicalOperation.NAND))
                            {
                                gateType = new LogicGateType(gate.Operation, connectionEntries.Count);
                                contains = logicGateRestrictions.Contains(gateType);
                            }
                        }

                        return contains;
                    }
                }
                else
                {
                    // Gate's opeartion is not alloved: return invalid.
                    return false;
                }
            }

            return true; // Good.
        }

        #endregion
    }
}
