using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public abstract class AdjustmentValues : IAdjustmentValueSource
    {
        protected AdjustmentValues(int size)
        {
            Contract.Requires(size > 0);

            Values = new double[size];
        }

        [DataMember]
        protected double[] Values { get; private set; }

        public int ValuesCount
        {
            [Pure]
            get { return Values.Length; }
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public class RatioValues : AdjustmentValues
    {
        public RatioValues(params double[] values) :
            this(false, values)
        {
        }

        public RatioValues(bool percents, params double[] values)
            : base(values.Length)
        {
            Contract.Requires(values.Length > 0);

            if (percents)
            {
                double sum = values.Sum();
                if (sum != 0.0)
                {
                    for (int i = 0; i < values.Length; i++) Values[i] = values[i] / sum;
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++) Values[i] = values[i];
            }
        }

        public double this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < ValuesCount);

                return Values[index];
            }
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class CMYKRatioValues : RatioValues
    {
        public CMYKRatioValues(double cRatio, double mRatio, double yRatio, double kRatio)
            : this(false, cRatio, mRatio, yRatio, kRatio)
        {
        }

        public CMYKRatioValues(bool percents, double cRatio, double mRatio, double yRatio, double kRatio)
            : base(percents, cRatio, mRatio, yRatio, kRatio)
        {
        }

        public double CRatio
        {
            get { return Values[0]; }
        }

        public double MRatio
        {
            get { return Values[1]; }
        }

        public double YRatio
        {
            get { return Values[2]; }
        }

        public double KRatio
        {
            get { return Values[3]; }
        }

        public override string ToString()
        {
            return string.Format("C:{0} M:{1} Y:{2} K:{3}", CRatio.ToString("0.0000"), MRatio.ToString("0.0000"), YRatio.ToString("0.0000"), KRatio.ToString("0.0000"));
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class RGBRatioValues : RatioValues
    {
        public RGBRatioValues(double rRatio, double gRatio, double bRatio)
            : this(false, rRatio, gRatio, bRatio)
        {
        }

        public RGBRatioValues(bool percents, double rRatio, double gRatio, double bRatio)
            : base(percents, rRatio, gRatio, bRatio)
        {
        }

        public double RRatio
        {
            get { return Values[0]; }
        }

        public double GRatio
        {
            get { return Values[1]; }
        }

        public double BRatio
        {
            get { return Values[2]; }
        }

        public override string ToString()
        {
            return string.Format("R:{0} G:{1} B:{2}", RRatio.ToString("0.0000"), GRatio.ToString("0.0000"), BRatio.ToString("0.0000"));
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public class AmmountValues : AdjustmentValues
    {
        public AmmountValues(double minAmmount, double maxAmmount, params double[] values)
            : base(values.Length)
        {
            Contract.Requires(values.Length > 0);

            for (int i = 0; i < values.Length; i++) Values[i] = values[i] * (maxAmmount - minAmmount) + minAmmount;
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class CMYKAmmountValues : AmmountValues
    {
        public CMYKAmmountValues(double minAmmount, double maxAmmount, double cRatio, double mRatio, double yRatio, double kRatio)
            : base(minAmmount, maxAmmount, cRatio, mRatio, yRatio, kRatio)
        {
        }

        public double CAmmount
        {
            get { return Values[0]; }
        }

        public double MAmmount
        {
            get { return Values[1]; }
        }

        public double YAmmount
        {
            get { return Values[2]; }
        }

        public double KAmmount
        {
            get { return Values[3]; }
        }

        public override string ToString()
        {
            return string.Format("C:{0} M:{1} Y:{2} K:{3}", CAmmount.ToString("0.0000"), MAmmount.ToString("0.0000"), YAmmount.ToString("0.0000"), KAmmount.ToString("0.0000"));
        }
    }

    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class RGBAmmountValues : AmmountValues
    {
        public RGBAmmountValues(double minAmmount, double maxAmmount, double rAmmount, double gAmmount, double bAmmount)
            : base(maxAmmount, rAmmount, gAmmount, bAmmount)
        {
        }

        public double RAmmount
        {
            get { return Values[0]; }
        }

        public double GAmmount
        {
            get { return Values[1]; }
        }

        public double BAmmount
        {
            get { return Values[2]; }
        }

        public override string ToString()
        {
            return string.Format("R:{0} G:{1} B:{2}", RAmmount.ToString("0.0000"), GAmmount.ToString("0.0000"), BAmmount.ToString("0.0000"));
        }
    }
}
