using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    public struct FeatureIndexSet : IEnumerable<int>
    {
        public FeatureIndexSet(IEnumerable<int> indexes, bool randomize = false)
        {
            Contract.Requires(indexes != null);

            this.indexes = new HashSet<int>(indexes);
            if (this.indexes.Count == 0) throw new ArgumentException("Index collection is empty.", "indexes");
            this.indexRange = null;
            this.randomize = randomize;
        }

        public FeatureIndexSet(int startIndex, int count, bool randomize = false)
        {
            Contract.Requires(startIndex >= 0);
            Contract.Requires(count > 0);

            this.indexRange = IntRange.CreateExclusive(startIndex, startIndex + count);
            this.indexes = null;
            this.randomize = randomize;
        }

        bool randomize;
        
        HashSet<int> indexes;

        IntRange? indexRange;

        internal HashSet<int> Indexes
        {
            get { return indexes; }
        }

        internal IntRange? IndexRange
        {
            get { return indexRange; }
        }

        internal bool Randomize
        {
            get { return randomize; }
        }

        public bool IsEmpty
        {
            [Pure]
            get { return indexes == null && indexRange == null; }
        }

        public int Count
        {
            [Pure]
            get { return (indexes != null) ? indexes.Count : (indexRange != null ? (indexRange.Value.MaxValue + 1) : 0); }
        }

        public IEnumerator<int> GetEnumerator()
        {
            if (indexes != null) return indexes.GetEnumerator();
            if (indexRange != null) return Enumerable.Range(indexRange.Value.MinValue, indexRange.Value.MaxValue - indexRange.Value.MinValue + 1).GetEnumerator();
            return Enumerable.Repeat(0, 0).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
