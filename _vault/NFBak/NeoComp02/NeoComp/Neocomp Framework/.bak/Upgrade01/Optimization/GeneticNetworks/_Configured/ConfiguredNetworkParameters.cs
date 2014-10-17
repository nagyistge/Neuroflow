using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class ConfiguredNetworkParameters : SynchronizedObject
    {
        bool cacheNetwork = true;

        public bool CacheNetwork
        {
            get { lock (SyncRoot) return cacheNetwork; }
            set { lock (SyncRoot) cacheNetwork = value; }
        }

        bool feedForward = true;

        public bool FeedForward
        {
            get { lock (SyncRoot) return feedForward; }
            set { lock (SyncRoot) feedForward = value; }
        }
        
        int maxConnectionIndex;

        public int MaxConnectionIndex
        {
            get { lock (SyncRoot) return maxConnectionIndex; }
            set
            {
                Contract.Requires(value > 0);

                lock (SyncRoot) maxConnectionIndex = value;
            }
        }

        IntRange connectionCountRange;

        public IntRange ConnectionCountRange
        {
            get { lock (SyncRoot) return connectionCountRange; }
            set
            {
                Contract.Requires(!value.IsZero);
                
                lock (SyncRoot) connectionCountRange = value;
            }
        }

        Probability indexMutationChance = 0.5;

        public Probability IndexMutationChance
        {
            get { lock (SyncRoot) return indexMutationChance; }
            set { lock (SyncRoot) indexMutationChance = value; }
        }

        internal bool CheckIsValid()
        {
            lock (SyncRoot)
            {
                return IsValid();
            }
        }

        protected virtual bool IsValid()
        {
            return maxConnectionIndex > 0 &&
                !connectionCountRange.IsZero;
        }
    }
}
