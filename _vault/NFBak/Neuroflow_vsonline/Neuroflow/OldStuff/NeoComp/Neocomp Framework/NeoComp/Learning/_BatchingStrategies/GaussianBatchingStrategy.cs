using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Algorithms.Selection;
using NeoComp.Core;

namespace NeoComp.Learning
{
    public sealed class GaussianBatchingStrategy : ErrorBasedBatchingStrategy
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

        public GaussianBatchingStrategy(double stdDev = 0.5)
        {
            Contract.Requires(stdDev > 0.0);

            StdDev = stdDev;

            algo = new GaussianSelectionAlgorithm(StdDev);
        } 

        #endregion

        #region Fields and Properties

        ISelectionAlgorithm algo;

        SortedList<Entry, object> entries;

        Entry[] outEntries;

        public double StdDev { get; private set; }

        #endregion

        #region Init

        protected override void Reinitialize()
        {
            int count = Batcher.ScriptProvider.Count;
            entries = new SortedList<Entry, object>(count, new EntryComparer());
            var randomNums = Enumerable.Range(0, count).OrderByRandom().GetEnumerator();
            for (int idx = 0; idx < count; idx++)
            {
                randomNums.MoveNext();
                int rowIdx = randomNums.Current;
                entries.Add(new Entry(rowIdx), null);
            }
            outEntries = new Entry[Batcher.BatchSize];
        }

        #endregion

        #region Get Next Logic

        protected internal override ISet<int> GetNextIndexes()
        {
            var selected = algo.Select(IntRange.CreateExclusive(0, entries.Count), Batcher.BatchSize);

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

        protected override void ErrorReportArrived(ScriptBatchExecutionResult result)
        {
            if (outEntries == null)
            {
                throw new InvalidOperationException(this + " is not initialized.");
            }

            if (result.Errors.Length != outEntries.Length)
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

                RegisterNewErrorInfo(entry, result.Errors[idx]);
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
