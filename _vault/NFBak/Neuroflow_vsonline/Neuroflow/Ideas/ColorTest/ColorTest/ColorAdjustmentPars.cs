using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;

namespace ColorTest
{
    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public abstract class ColorAdjustmentPars : IAdjustmentValueSource
    {
        protected const int SplineResolution = (256 * 4) - 1;

        protected const int SplineControlPoints = 6;

        const double MaxPowAmmount = 3.0;

        const double MaxMulAmmount = 3.0;

        public int ValuesCount
        {
            get
            {
                return (from pi in GetType().GetProperties()
                        where typeof(IAdjustmentValueSource).IsAssignableFrom(pi.PropertyType)
                        let v = (IAdjustmentValueSource)pi.GetValue(this, null)
                        where v != null
                        select v.ValuesCount).Sum();
            }
        }

        protected static CMYKAmmountValues CreateDefaultCMYKPowValues()
        {
            return CreateCMYKPowValues(1.0 / MaxPowAmmount, 1.0 / MaxPowAmmount, 1.0 / MaxPowAmmount, 1.0 / MaxPowAmmount);
        }

        protected static RGBAmmountValues CreateDefaultRGBPowValues()
        {
            return CreateRGBPowValues(1.0 / MaxPowAmmount, 1.0 / MaxPowAmmount, 1.0 / MaxPowAmmount);
        }

        protected static CMYKAmmountValues CreateDefaultCMYKMulValues()
        {
            return CreateCMYKMulValues(1.0 / MaxMulAmmount, 1.0 / MaxMulAmmount, 1.0 / MaxMulAmmount, 1.0 / MaxMulAmmount);
        }

        protected static CMYKRatioValues CreateDefaultCMYKRatios()
        {
            return CreateCMYKRatios(1.0, 1.0, 1.0, 1.0);
        }

        public static CMYKAmmountValues CreateCMYKPowValues(double forC, double forM, double forY, double forK)
        {
            return new CMYKAmmountValues(double.Epsilon, MaxPowAmmount, forC, forM, forY, forK);
        }

        public static RGBAmmountValues CreateRGBPowValues(double forR, double forG, double forB)
        {
            return new RGBAmmountValues(double.Epsilon, MaxPowAmmount, forR, forG, forB);
        }

        public static CMYKAmmountValues CreateCMYKMulValues(double forC, double forM, double forY, double forK)
        {
            return new CMYKAmmountValues(0.0, MaxMulAmmount, forC, forM, forY, forK);
        }

        public static CMYKRatioValues CreateCMYKRatios(double forC, double forM, double forY, double forK)
        {
            return new CMYKRatioValues(false, forC, forM, forY, forK);
        }
    }
}
