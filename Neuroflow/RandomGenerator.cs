using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow
{
    public static class RandomGenerator
    {
        const uint CountFrom = 100000000;

        const int ReinitOn = 10;

        static Random seed = new Random();
        
        [ThreadStatic]
        static Random rnd;

        [ThreadStatic]
        static uint counter;

        public static Random Random
        {
            get
            {
                if (rnd == null)
                {
                    int s;
                    lock (seed) s = seed.Next();
                    rnd = new Random(s);
                    counter = CountFrom;
                }
                else if (--counter == 0)
                {
                    int tc = Environment.TickCount;
                    if (tc % ReinitOn == 0)
                    {
                        int s;
                        lock (seed) s = seed.Next();
                        rnd = new Random(s);
                    }
                    counter = CountFrom;
                }
                return rnd;
            }
        }
        
        public static bool FiftyPercentChance
        {
            get { return Random.Next(2) == 0; }
        }

        public static double NextDouble(double min, double max)
        {
            double v = Random.NextDouble(); // 0..1
            double d = max - min;
            return v * d + min;
        }

        public static IEnumerable<double> NextDoubles(double min, double max, int count)
        {
            return Enumerable.Range(0, count).Select(i => NextDouble(min, max));
        }

        public static float NextFloat(float min, float max)
        {
            float v = (float)Random.NextDouble(); // 0..1
            float d = max - min;
            return v * d + min;
        }

        public static IEnumerable<float> NextFloats(float min, float max, int count)
        {
            return Enumerable.Range(0, count).Select(i => NextFloat(min, max));
        }

        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> items)
        {
            Contract.Requires(items != null);

            return items.OrderBy(i => Random.Next());
        }
    }
}
