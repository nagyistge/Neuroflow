using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    internal class WeightHistoryEntry
    {
        internal WeightHistoryEntry(double weight, double error)
        {
            Weight = weight;
            Error = error;
            double e = error;
            if (e < 0.0) e = 0.0; else if (e > 1.0) e = 1.0;
            Rank = 1.0 - e;
        }
        
        internal double Weight { get; private set; }

        internal double Error { get; private set; }

        internal double Rank { get; private set; }
    }
    
    internal class WeightHistory
    {
        internal WeightHistory(int resolution, double weightRange)
        {
            Contract.Requires(resolution > 1);
            Contract.Requires(weightRange > 0.0);

            Resolution = resolution;
            this.weightRange = new DoubleRange(-weightRange, weightRange);
            entries = new SortedList<double, WeightHistoryEntry>();
            var last = CreateLastEntry();
            entries.Add(last.Rank, last);
        }

        DoubleRange weightRange;

        SortedList<double, WeightHistoryEntry> entries;
        
        internal int Resolution { get; private set; }

        internal bool IsFull
        {
            get { return entries.Count == Resolution; }
        }

        private WeightHistoryEntry CreateLastEntry()
        {
            return new WeightHistoryEntry(weightRange.PickRandomValue(), 1.0);
        }
    }
    
    public sealed class HistoryAlgorithm : GlobalOptimizationLearningAlgorithm
    {
    }
}
