using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public sealed class FitnessFunctionComparer : IComparer<Genom>
    {
        public FitnessFunctionComparer(Func<Genom, double> fitnessFunction)
        {
            Contract.Requires(fitnessFunction != null);

            this.fitnessFunction = fitnessFunction;
        }

        Func<Genom, double> fitnessFunction;

        public int Compare(Genom x, Genom y)
        {
            return fitnessFunction(x).CompareTo(fitnessFunction(y));
        }
    }
}
