using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Collections.ObjectModel;

namespace NeoComp.Features
{
    public sealed class AllDataFeatureSelectionStrategy : DataFeatureSelectionStrategy
    {
        public AllDataFeatureSelectionStrategy(bool randomize = true, bool requestAlways = false)
        {
            Randomize = randomize;
            RequestAlways = requestAlways;
        }

        bool retrieved;

        public bool Randomize { get; private set; }

        public bool RequestAlways { get; private set; }
        
        protected override void Initialize()
        {
            retrieved = false;
        }

        protected internal override void Uninitialize()
        {
            retrieved = false;
        }

        protected internal override FeatureIndexSet? GetNextIndexes()
        {
            if (retrieved && !RequestAlways) return null;

            var indexes = Enumerable.Range(0, Owner.DataFeatureProvider.ItemCount);
            retrieved = true;
            return new FeatureIndexSet(indexes, Randomize);
        }
    }
}
