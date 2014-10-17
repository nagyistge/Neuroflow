using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public class Population : SortedList<SolutionKey, double[]>
    {
        public Population(int size) 
            : base(size)
        {
            Contract.Requires(size > 0);

            Size = size;
        }

        public int Size { get; private set; }

        public void Add(double mse, double[] weights)
        {
            Contract.Requires(mse >= 0.0);
            Contract.Requires(weights != null && weights.Length > 0);

            Add(new SolutionKey(mse), weights);
        }
    }
}
