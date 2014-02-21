using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThirdParty.SharpNeatLib.Maths;
using System.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public static class Statistics
    {
        public static double GenerateGauss(double mean, double stdDev)
        {
            double next = GetNextGauss();
            return (next * stdDev) + mean;
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

        public static double CalculateStdDev(this IEnumerable<double> numbers)
        {
            Contract.Requires(numbers != null);

            double average = 0.0;
            double sumOfDerivation = 0.0;
            double count = 0.0;
            foreach (double value in numbers)
            {
                sumOfDerivation += (value) * (value);
                average += value;
                count++;
            }
            average /= count;
            double sumOfDerivationAverage = sumOfDerivation / count;
            return Math.Sqrt(sumOfDerivationAverage - (average * average));  
        }

        static double GetNextGauss()
        {
            double fac, rsq, v1, v2;

            do
            {
                v1 = 2.0 * RandomGenerator.Random.NextDouble() - 1.0;
                v2 = 2.0 * RandomGenerator.Random.NextDouble() - 1.0;
                rsq = v1 * v1 + v2 * v2;
            }
            while (rsq >= 1.0 || rsq == 0.0);

            fac = Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);
            return v1 * fac;
        } 
    }
}
