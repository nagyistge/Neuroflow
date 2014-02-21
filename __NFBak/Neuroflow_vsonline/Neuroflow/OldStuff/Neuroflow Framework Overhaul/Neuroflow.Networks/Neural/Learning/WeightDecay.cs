using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Neuroflow.Networks.Neural.Learning
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

    public sealed class WeightDecay
    {
        const double DefCutoff = 2.0;
        const double DefFactor = -0.0001;

        public WeightDecay()
        {
            Cutoff = DefCutoff;
            Factor = DefFactor;
            IsEnabled = true;
        }

        double cutoff;
        double cutoff4;

        [Category(PropertyCategories.Math)]
        [InitValue(DefCutoff)]
        [DefaultValue(DefCutoff)]
        [Required]
        public double Cutoff
        {
            get { return cutoff; }
            set
            {
                cutoff = value;
                cutoff4 = Math.Pow(value, 4.0);
            }
        }

        [Category(PropertyCategories.Math)]
        [InitValue(DefFactor)]
        [DefaultValue(DefFactor)]
        [Required]
        public double Factor { get; set; }

        [Category(PropertyCategories.Behavior)]
        [InitValue(true)]
        [DefaultValue(true)]
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
