using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
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

        public double Count { get; private set; }

        public double AvgGradient
        {
            get { return Sum.Gradient / Count; }
        }

        public double AvgErrorSum
        {
            get { return Sum.ErrorSum / Count; }
        }

        public void AddNext(double error, double input, bool isNewBatch)
        {
            Last.Set(error, input);
            if (isNewBatch)
            {
                Sum.Set(error, input);
                Count = 1;
            }
            else
            {
                Sum.Add(error, input);
                Count++;
            }
        }
    }
}
