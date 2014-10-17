using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public static class WeightDecayExt
    {
        public static double Decayed(this WeightDecay decay, double weight)
        {
            if (decay != null)
            {
                return decay.GetDecayed(weight);
            }
            return weight;
        }
    }
    
    public class WeightDecay
    {
        double cutoff = 2.0;
        double cutoff4 = Math.Pow(2.0, 4.0);

        public double Cutoff
        {
            get { return cutoff; }
            set
            {
                Contract.Requires(value >= 0.0);

                cutoff = value;
                cutoff4 = Math.Pow(value, 4.0);
            }
        }

        double factor = -0.00001;

        public double Factor
        {
            get { return factor; }
            set
            {
                Contract.Requires(value <= 0.0);

                factor = value;
            }
        }

        public bool IsEnabled { get; set; }

        internal double GetDecayed(double weight)
        {
            if (IsEnabled)
            {
                double w4 = Math.Pow(weight, 4.0);
                double div = w4 + cutoff4;
                if (div != 0.0)
                {
                    double delta = (w4 / div) * Factor * weight;
                    weight += delta;
                }
            }
            return weight;
        }
    }
}
