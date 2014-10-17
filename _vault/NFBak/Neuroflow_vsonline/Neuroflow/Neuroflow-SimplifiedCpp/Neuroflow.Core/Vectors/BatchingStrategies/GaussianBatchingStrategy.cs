using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Core.Algorithms.Selection;

namespace Neuroflow.Core.Vectors.BatchingStrategies
{
    public sealed class GaussianBatchingStrategy : OptimizationBatchingStrategy
    {
        #region Entry Stuff

        sealed class Entry
        {
            static int counter = 0;

            internal Entry(int rowIndex, double mse = double.MaxValue)
            {
                Contract.Requires(rowIndex >= 0);
                Contract.Requires(mse >= 0.0);

                this.rowIndex = rowIndex;
                this.mse = mse;
                this.id = counter++;
            }

            internal Entry(Entry baseEntry, double mse = double.MaxValue)
            {
                Contract.Requires(mse >= 0.0);
                Contract.Requires(baseEntry.RowIndex >= 0);

                this.rowIndex = baseEntry.RowIndex;
                this.mse = mse;
                this.id = baseEntry.ID;
            }

            int id, rowIndex;

            double mse;

            internal int ID
            {
                get { return id; }
            }

            internal int RowIndex
            {
                get { return rowIndex; }
            }

            internal double MSE
            {
                get { return mse; }
            }
        }

        sealed class EntryComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                int c = x.MSE.CompareTo(y.MSE);
                if (c != 0) return -c;
                return x.ID.CompareTo(y.ID);
            }
        }

        #endregion

        #region Constructor

        public GaussianBatchingStrategy(
            [InitValue(0.5)]
            [Category(PropertyCategories.Math)]
            [FreeDisplayName("Std. Dev.")]
            double stdDev = 0.5)
        {
            Contract.Requires(stdDev > 0.0);

            StdDev = stdDev;

            algo = new GaussianSelectionAlgorithm(StdDev);
        }

        #endregion

        #region Fields and Properties

        GaussianSelectionAlgorithm algo;

        SortedList<Entry, object> entries;

        Entry[] outEntries;

        public double StdDev { get; private set; }

        #endregion

        #region Init

        protected override void DoInitialize()
        {
            entries = new SortedList<Entry, object>(ItemCount, new EntryComparer());
            var randomNums = Enumerable.Range(0, ItemCount).OrderByRandom().GetEnumerator();
            for (int idx = 0; idx < ItemCount; idx++)
            {
                randomNums.MoveNext();
                int rowIdx = randomNums.Current;
                entries.Add(new Entry(rowIdx), null);
            }
            outEntries = new Entry[BatchSize];
        }

        #endregion

        #region Get Next Logic

        protected override ISet<int> DoGetNextIndexes()
        {
            var selected = algo.Select(IntRange.CreateExclusive(0, entries.Count), BatchSize);

            int oidx = 0;
            foreach (int idx in selected)
            {
                var entry = entries.Keys[idx];
                outEntries[oidx++] = entry;
            }
            return new HashSet<int>(outEntries.Select(e => e.RowIndex));
        }

        #endregion

        #region Error Report

        public override void SetLastResult(BatchExecutionResult lastResult)
        {
            if (outEntries == null)
            {
                throw new InvalidOperationException(this + " is not initialized.");
            }

            if (lastResult.Errors.Length != outEntries.Length)
            {
                throw new InvalidOperationException("Reported error vector length is invalid.");
            }

            // Register error result.
            for (int idx = 0; idx < outEntries.Length; idx++)
            {
                var entry = outEntries[idx];

                if (entry == null)
                {
                    throw new InvalidOperationException("There are no entries out.");
                }

                RegisterNewErrorInfo(entry, lastResult.Errors[idx]);
            }
        }

        private void RegisterNewErrorInfo(Entry entry, double error)
        {
            var newEntry = new Entry(entry, error);

            try
            {
                entries.Remove(entry);
                entries.Add(newEntry, null);
            }
            catch (Exception ex)
            {
                var ex2 = new InvalidOperationException("Entry not found. See provided Data 'entry' and inner exception for details.", ex);
                ex2.Data["entry"] = entry;
                throw ex2;
            }
        }

        #endregion
    }
}
