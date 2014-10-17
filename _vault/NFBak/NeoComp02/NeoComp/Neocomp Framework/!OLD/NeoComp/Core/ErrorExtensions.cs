using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public static class ErrorExtensions
    {
        public static double Sum(this IEnumerable<IEnumerable<double>> errors)
        {
            Contract.Requires(errors != null);

            double e = 0.0;
            foreach (var subErrors in errors)
            {
                if (subErrors != null) e += subErrors.Select(v => Math.Abs(v)).Sum();
            }
            return e;
        }

        public static double Average(this IEnumerable<IEnumerable<double>> errors)
        {
            Contract.Requires(errors != null);

            double count = 0.0;
            double error = 0.0;
            foreach (var e in errors)
            {
                if (e != null)
                {
                    error += e.Select(v => Math.Abs(v)).Average();
                    count++;
                }
            }
            return count == 0.0 ? 0.0 : error / count;
        }

        public static double MeanSquare(this IEnumerable<double> values)
        {
            Contract.Requires(values != null);

            int count = 0;
            double sum = 0.0;
            foreach (var value in values)
            {
                sum += value * value;
                count++;
            }
            return count == 0 ? 0.0 : sum / (double)count;
        }

        public static double MeanSquare(this IEnumerable<IEnumerable<double>> values)
        {
            Contract.Requires(values != null);

            int count = 0;
            double sum = 0.0;
            foreach (var subValues in values)
            {
                if (subValues != null)
                {
                    foreach (var value in subValues)
                    {
                        sum += value * value;
                        count++;
                    }
                }
            }
            return count == 0 ? 0.0 : sum / (double)count;
        }
    }
}
