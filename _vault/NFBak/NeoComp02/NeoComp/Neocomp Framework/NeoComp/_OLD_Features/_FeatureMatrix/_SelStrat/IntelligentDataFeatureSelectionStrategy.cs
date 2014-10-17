using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Computations;
using NeoComp.Optimization.Algorithms.Selection;
using System.Threading;

namespace NeoComp.Features
{
    /// <summary>
    /// Intelligent Feature Selection Strategy
    /// Thx to Juhi!
    /// </summary>
    public sealed class IntelligentDataFeatureSelectionStrategy : DataFeatureSelectionStrategy, IFeatureErrorReport
    {
        #region Entry Stuff

        /// <summary>
        /// Entry struct for key.
        /// </summary>
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

        #region Constructors

        public IntelligentDataFeatureSelectionStrategy(int groupSize, ISelectionAlgorithm selectionAlgorithm = null, int resetFrequency = 1000, MTPEliminationParameters mtpEliminationParameters = null)
        {
            Contract.Requires(groupSize > 0);
            Contract.Requires(resetFrequency >= 0);

            ResetFrequency = resetFrequency;
            GroupSize = groupSize;
            this.selectionAlgorithm = selectionAlgorithm == null ? new BorderSelectionAlgorithm() : selectionAlgorithm;
            MTPEliminationParameters = mtpEliminationParameters;
        }

        #endregion

        #region Fields and Props

        public int GroupSize { get; private set; }

        public int ResetFrequency { get; private set; }

        public MTPEliminationParameters MTPEliminationParameters { get; private set; }

        int resetCounter;

        int mtpCounter;

        double lastMTPError = double.MaxValue;

        ISelectionAlgorithm selectionAlgorithm;

        SortedList<Entry, object> entries;

        Entry[] outEntries;

        #endregion

        #region Init / Uninit

        protected override void Initialize()
        {
            int count = Owner.DataFeatureProvider.ItemCount;
            if (GroupSize > count / 2) GroupSize = count / 2;
            entries = new SortedList<Entry, object>(count, new EntryComparer());
            var randomNums = Enumerable.Range(0, count).OrderByRandom().GetEnumerator();
            for (int idx = 0; idx < count; idx++)
            {
                randomNums.MoveNext();
                int rowIdx = randomNums.Current;
                entries.Add(new Entry(rowIdx), null);
            }
            outEntries = new Entry[GroupSize];
            resetCounter = 0;
            mtpCounter = 0;
        }

        protected internal override void Uninitialize()
        {
            entries = null;
            outEntries = null;
        }

        #endregion

        #region Select

        protected internal override FeatureIndexSet? GetNextIndexes()
        {
            if (mtpCounter != 0)
            {
                // In Moving Target Elimination iteration.
                return null;
            }
            
            if (ResetFrequency != 0 && resetCounter++ == ResetFrequency)
            {
                Reset();
            }
            
            var selected = selectionAlgorithm.Select(IntRange.CreateExclusive(0, entries.Count), GroupSize);
            int oidx = 0;
            foreach (int idx in selected)
            {
                var entry = entries.Keys[idx];
                outEntries[oidx++] = entry;
            }
            var result = new FeatureIndexSet(outEntries.Select(e => e.RowIndex));

            if (MTPEliminationParameters != null && entries.Keys[0].MSE != double.MaxValue)
            {
                // Begin Moving Target Elimination iteration.
                mtpCounter = 1;
                lastMTPError = double.MaxValue;
            }

            return result;
        }

        #endregion

        #region Report

        private void ReportArrived(Vector<double> errors)
        {
            if (outEntries == null)
            {
                throw new InvalidOperationException(this + " is not initialized.");
            }

            if (errors.Dimension != outEntries.Length)
            {
                throw new InvalidOperationException("Reported error vector length is invalid.");
            }

            // MTP Elimination
            if (mtpCounter != 0)
            {
                // Begin Moving Target Elimination iteration report.
                if (mtpCounter < MTPEliminationParameters.MinIterations)
                {
                    mtpCounter++;
                    return;
                }
                else
                {
                    double? avgN = errors.Average();
                    double avg = avgN.HasValue ? avgN.Value : 0.0;
                    
                    if (mtpCounter == MTPEliminationParameters.MinIterations)
                    {
                        lastMTPError = avg;
                        mtpCounter++;
                        return;
                    }

                    if (mtpCounter < MTPEliminationParameters.MaxIterations && avg <= lastMTPError)
                    {
                        // Improved.
                        lastMTPError = avg;
                        mtpCounter++;
                        return;
                    }
                    
                    mtpCounter = 0;
                }
            }

            // Register error result.
            for (int idx = 0; idx < outEntries.Length; idx++)
            {
                var entry = outEntries[idx];

                if (entry == null)
                {
                    throw new InvalidOperationException("There are no entries out.");
                }
                
                double? error = errors.ItemArray[idx];
                if (error == null) error = entry.MSE;
                RegisterNewErrorInfo(entry, error.Value);
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

        #region Reset

        private void Reset()
        {
            var newEntries = new SortedList<Entry, object>(entries.Count, new EntryComparer());
            foreach (var kvp in entries) newEntries.Add(new Entry(kvp.Key), kvp.Value);
            entries = newEntries;
            resetCounter = 0;
            mtpCounter = 0;
        }

        #endregion

        #region IFeatureErrorReport

        void IFeatureErrorReport.ReportError(Vector<double> errors)
        {
            ReportArrived(errors);
        }

        #endregion
    }
}
