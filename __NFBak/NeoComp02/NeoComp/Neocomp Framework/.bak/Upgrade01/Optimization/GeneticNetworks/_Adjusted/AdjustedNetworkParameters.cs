using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class AdjustedNetworkParameters : SynchronizedObject
    {
        internal IAdjustableItem[] AdjustableItems { get; private set; }
        
        INetwork network;

        public INetwork Network
        {
            get { lock (SyncRoot) return network; }
            set
            {
                Contract.Requires(value != null);

                lock (SyncRoot)
                {
                    network = value;
                    lock (network.SyncRoot)
                    {
                        AdjustableItems = NetworkExtensions.GetAdjustableItems(network).ToArray();
                    }
                }
            }
        }

        double? mutationStrength;

        public double? MutationStrength
        {
            get { lock (SyncRoot) return mutationStrength; }
            set
            {
                Contract.Requires(!value.HasValue || (value.Value > 0.0 && value <= 1.0));

                lock (SyncRoot)
                {
                    this.mutationStrength = value;
                }
            }
        }

        internal bool CheckIsValid()
        {
            lock (SyncRoot) return IsValid();
        }

        protected virtual bool IsValid()
        {
            return network != null;
        }
    }
}
