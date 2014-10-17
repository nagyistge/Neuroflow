using Devcorp.Controls.Design;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorTest
{
    public static class ColorMixer
    {
        public static HdrRGB Mix(ColorFilteringPars filteringPars, ColorMixingPars mixingPars, WeightedValue<HdrRGB>[] colors)
        {
            Contract.Requires(filteringPars != null);
            Contract.Requires(mixingPars != null);
            Contract.Requires(colors != null);
            Contract.Requires(colors.Length > 0);

            double sumWeightC = 0.0;
            double sumWeightM = 0.0;
            double sumWeightY = 0.0;
            double sumWeightK = 0.0;
            double weight;

            double cAvg = 0.0, mAvg = 0.0, yAvg = 0.0, kAvg = 0.0;

            foreach (var color in colors)
            {
                HdrCMYK cmyk = filteringPars.Filter(color.Value);

                // Avg
                var pars = mixingPars.AvgWeightPars.CPars;
                weight = pars.SplineValues.CValues.GetY(cmyk.C, 1.0) * pars.Ratios.CRatio +
                         pars.SplineValues.MValues.GetY(cmyk.M, 1.0) * pars.Ratios.MRatio +
                         pars.SplineValues.YValues.GetY(cmyk.Y, 1.0) * pars.Ratios.YRatio +
                         pars.SplineValues.KValues.GetY(cmyk.K, 1.0) * pars.Ratios.KRatio;
                weight *= color.Weight;
                sumWeightC += weight;
                cAvg += cmyk.C * weight;

                pars = mixingPars.AvgWeightPars.MPars;
                weight = pars.SplineValues.CValues.GetY(cmyk.C, 1.0) * pars.Ratios.CRatio +
                         pars.SplineValues.MValues.GetY(cmyk.M, 1.0) * pars.Ratios.MRatio +
                         pars.SplineValues.YValues.GetY(cmyk.Y, 1.0) * pars.Ratios.YRatio +
                         pars.SplineValues.KValues.GetY(cmyk.K, 1.0) * pars.Ratios.KRatio;
                weight *= color.Weight;
                sumWeightM += weight;
                mAvg += cmyk.M * weight;

                pars = mixingPars.AvgWeightPars.YPars;
                weight = pars.SplineValues.CValues.GetY(cmyk.C, 1.0) * pars.Ratios.CRatio +
                         pars.SplineValues.MValues.GetY(cmyk.M, 1.0) * pars.Ratios.MRatio +
                         pars.SplineValues.YValues.GetY(cmyk.Y, 1.0) * pars.Ratios.YRatio +
                         pars.SplineValues.KValues.GetY(cmyk.K, 1.0) * pars.Ratios.KRatio;
                weight *= color.Weight;
                sumWeightY += weight;
                yAvg += cmyk.Y * weight;

                pars = mixingPars.AvgWeightPars.KPars;
                weight = pars.SplineValues.CValues.GetY(cmyk.C, 1.0) * pars.Ratios.CRatio +
                         pars.SplineValues.MValues.GetY(cmyk.M, 1.0) * pars.Ratios.MRatio +
                         pars.SplineValues.YValues.GetY(cmyk.Y, 1.0) * pars.Ratios.YRatio +
                         pars.SplineValues.KValues.GetY(cmyk.K, 1.0) * pars.Ratios.KRatio;
                weight *= color.Weight;
                sumWeightK += weight;
                kAvg += cmyk.K * weight;
            }

            // Avg finish
            cAvg = sumWeightC == 0.0 ? 0.0 : (cAvg / sumWeightC);
            mAvg = sumWeightM == 0.0 ? 0.0 : (mAvg / sumWeightM);
            yAvg = sumWeightY == 0.0 ? 0.0 : (yAvg / sumWeightY);
            kAvg = sumWeightK == 0.0 ? 0.0 : (kAvg / sumWeightK);

            // Blend results to RGB

            //var rgb = ToRGB(mixingPars, cAvg, mAvg, yAvg, kAvg);
            HdrRGB rgb = new HdrCMYK(cAvg, mAvg, yAvg, kAvg);

            // Color filter
            rgb = filteringPars.UndoFilter(rgb);

            return rgb;
        }
    }
}
