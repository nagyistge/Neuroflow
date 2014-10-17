using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    internal struct Entropy
    {
        internal Entropy(double mean, double stdDev)
        {
            Contract.Requires(stdDev >= 0.0);

            this.mean = mean;
            this.stdDev = stdDev;
        }
        
        double mean, stdDev;

        internal double Mean
        {
            get { return mean; }
        }

        internal double StdDev
        {
            get { return stdDev; }
        }
    }
}
