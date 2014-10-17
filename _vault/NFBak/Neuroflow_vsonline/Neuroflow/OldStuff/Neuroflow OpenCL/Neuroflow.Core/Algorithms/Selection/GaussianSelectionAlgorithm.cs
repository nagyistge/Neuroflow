using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Algorithms.Selection
{
    public sealed class GaussianSelectionAlgorithm : SelectionAlgorithm
    {
        public GaussianSelectionAlgorithm(double stdDev = 0.5, SelectionDirection direction = SelectionDirection.FromTop)
        {
            Contract.Requires(stdDev > 0.0);

            Direction = direction;
            StdDev = stdDev;
        }

        public SelectionDirection Direction { get; private set; }

        public double StdDev { get; private set; }
        
        protected override int GetNext(IntRange fromRange, int soFar)
        {
            double value = Generate(); // Middle: 0.0
            if (value > 1.0) value = 1.0; else if (value < -1.0) value = -1.0; // Range: -1.0 .. 1.0, Middle: 0.0
            if (value < 0.0) value = -value; // Range: 0.0 .. 1.0, Highest: 0.0
            if (Direction == SelectionDirection.FromBottom) value = (1.0 - value); // Range: 0.0 .. 1.0, Highest: 1.0

            double min = fromRange.MinValue;
            double max = fromRange.MaxValue;
            double d = max - min;
            int result = (int)Math.Round(d * value + min);
            return result;
        }

        double Generate()
        {
            return Statistics.GenerateGauss(0.0, StdDev);
        }
    }
}
