using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Computations;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    public sealed class BackwardValues
    {
        public BackwardValues()
        {
            Sum = new BackwardValue();
            Last = new BackwardValue();
        }
        
        public BackwardValue Sum { get; private set; }

        public BackwardValue Last { get; private set; }

        public int Count { get; private set; }

        public double AvgGradient
        {
            get { return Sum.Gradient / (double)Count; }
        }

        public void Set(double error, double gradient = 0.0)
        {
            Last.Set(error, gradient);
        }

        public void Add(double error, double gradient)
        {
            Last.Set(error, gradient);
            Sum.Add(error, gradient);
            Count++;
        }

        internal void ResetTracking()
        {
            Sum.Reset();
            Count = 0;
        }

        internal void ResetLastError()
        {
            Last.Reset();
        }
    }
}
