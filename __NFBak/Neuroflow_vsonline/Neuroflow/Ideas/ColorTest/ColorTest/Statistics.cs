using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core
{
    public static class Statistics
    {
        [ThreadStatic]
        private static double? nextGauss;

        public static double GenerateGauss(double mean, double stdDev)
        {
            double next = GetNextGauss();
            return (next * stdDev) + mean;
        }

        private static double GetNextGauss()
        {
            if (nextGauss.HasValue)
            {
                var v = nextGauss.Value;
                nextGauss = null;
                return v;
            }
            else
            {
                var rnd = RandomGenerator.Random;
                double v1, v2, s;
                do
                {
                    v1 = 2.0 * rnd.NextDouble() - 1.0;
                    v2 = 2.0 * rnd.NextDouble() - 1.0;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1.0 || s == 0);
                s = System.Math.Sqrt((-2.0 * System.Math.Log(s)) / s);
                nextGauss = v2 * s;
                return v1 * s;
            }
        }

        public static void CalculateMeanAndStdDev(this IEnumerable<double> numbers, out double mean, out double stdDev)
        {
            Contract.Requires(numbers != null);
            mean = 0.0;
            double sumOfDerivation = 0.0;
            double count = 0.0;
            foreach (double value in numbers)
            {
                sumOfDerivation += (value) * (value);
                mean += value;
                count++;
            }
            mean /= count;
            double sumOfDerivationAverage = sumOfDerivation / count;
            stdDev = Math.Sqrt(sumOfDerivationAverage - (mean * mean));
        }
    }
}
