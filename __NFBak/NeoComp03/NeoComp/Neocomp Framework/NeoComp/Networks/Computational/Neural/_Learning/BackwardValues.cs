using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class BackwardValues : IReset
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

        public void Reset()
        {
            Sum.Reset();
            Last.Reset();
            Count = 0;
        }

        internal void ResetErros()
        {
            Last.Reset();
        }
    }
}
