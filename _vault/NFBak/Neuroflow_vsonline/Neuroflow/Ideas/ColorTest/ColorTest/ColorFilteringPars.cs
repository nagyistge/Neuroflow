using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class ColorFilteringPars : ColorAdjustmentPars
    {
        const double MulV = 2.0;

        const double PowV = 2.0;
        
        const double AddV = 80.0, AddV2 = AddV / 2.0;

        [DataMember(Name = "ConstRatio", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        RGBRatioValues constRatio;

        public RGBRatioValues ConstRatio
        {
            get { return constRatio ?? (constRatio = new RGBRatioValues(1.0, 1.0, 1.0)); }
            set
            {
                Contract.Requires(value != null);

                constRatio = value;
            }
        }

        [DataMember(Name = "MulRatio", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        RGBRatioValues mulRatio;

        public RGBRatioValues MulRatio
        {
            get { return mulRatio ?? (mulRatio = new RGBRatioValues(1.0, 1.0, 1.0)); }
            set
            {
                Contract.Requires(value != null);

                mulRatio = value;
            }
        }

        [DataMember(Name = "InnerMulRatio", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        RGBRatioValues innerMulRatio;

        public RGBRatioValues InnerMulRatio
        {
            get { return innerMulRatio ?? (innerMulRatio = new RGBRatioValues(1.0, 1.0, 1.0)); }
            set
            {
                Contract.Requires(value != null);

                innerMulRatio = value;
            }
        }

        [DataMember(Name = "PowRatio", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        RGBRatioValues powRatio;

        public RGBRatioValues PowRatio
        {
            get { return powRatio ?? (powRatio = new RGBRatioValues(1.0, 1.0, 1.0)); }
            set
            {
                Contract.Requires(value != null);

                powRatio = value;
            }
        }

        public void Init(double[] values, ref int valueIndex)
        {
            ConstRatio = new RGBRatioValues(values[valueIndex++], values[valueIndex++], values[valueIndex++]);
            MulRatio = new RGBRatioValues(values[valueIndex++], values[valueIndex++], values[valueIndex++]);
            InnerMulRatio = new RGBRatioValues(values[valueIndex++], values[valueIndex++], values[valueIndex++]);
            PowRatio = new RGBRatioValues(values[valueIndex++], values[valueIndex++], values[valueIndex++]);
        }

        public HdrRGB Filter(HdrRGB color)
        {
            double fPMR = Math.Max(MulRatio.RRatio * MulV, double.Epsilon);
            double fPMG = Math.Max(MulRatio.GRatio * MulV, double.Epsilon);
            double fPMB = Math.Max(MulRatio.BRatio * MulV, double.Epsilon);

            double fPIMR = Math.Max(InnerMulRatio.RRatio * MulV, double.Epsilon);
            double fPIMG = Math.Max(InnerMulRatio.GRatio * MulV, double.Epsilon);
            double fPIMB = Math.Max(InnerMulRatio.BRatio * MulV, double.Epsilon);

            double fPPR = PowRatio.RRatio * PowV;
            double fPPG = PowRatio.GRatio * PowV;
            double fPPB = PowRatio.BRatio * PowV;

            double fPCR = ConstRatio.RRatio * AddV - AddV2;
            double fPCG = ConstRatio.GRatio * AddV - AddV2;
            double fPCB = ConstRatio.BRatio * AddV - AddV2;

            double r = (Math.Pow(color.R * fPIMR, fPPR) * fPMR) + fPCR;
            double g = (Math.Pow(color.G * fPIMG, fPPG) * fPMG) + fPCG;
            double b = (Math.Pow(color.B * fPIMB, fPPB) * fPMB) + fPCB;

            return new HdrRGB(r, g, b);
        }

        public HdrRGB UndoFilter(HdrRGB color)
        {
            double fPMR = Math.Max(MulRatio.RRatio * MulV, double.Epsilon);
            double fPMG = Math.Max(MulRatio.GRatio * MulV, double.Epsilon);
            double fPMB = Math.Max(MulRatio.BRatio * MulV, double.Epsilon);

            double fPIMR = Math.Max(InnerMulRatio.RRatio * MulV, double.Epsilon);
            double fPIMG = Math.Max(InnerMulRatio.GRatio * MulV, double.Epsilon);
            double fPIMB = Math.Max(InnerMulRatio.BRatio * MulV, double.Epsilon);

            double fPPR = PowRatio.RRatio * PowV;
            double fPPG = PowRatio.GRatio * PowV;
            double fPPB = PowRatio.BRatio * PowV;

            double fPCR = ConstRatio.RRatio * AddV - AddV2;
            double fPCG = ConstRatio.GRatio * AddV - AddV2;
            double fPCB = ConstRatio.BRatio * AddV - AddV2;

            double r = Math.Pow(Math.Max((color.R - fPCR) / fPMR, 0.0), 1.0 / fPPR) / fPIMR;
            double g = Math.Pow(Math.Max((color.G - fPCG) / fPMG, 0.0), 1.0 / fPPG) / fPIMG;
            double b = Math.Pow(Math.Max((color.B - fPCB) / fPMB, 0.0), 1.0 / fPPB) / fPIMB;

            return new HdrRGB(r, g, b);
        }
    }
}
