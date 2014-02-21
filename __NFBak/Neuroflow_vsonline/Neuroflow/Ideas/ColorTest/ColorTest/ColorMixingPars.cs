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
    public sealed class ColorMixingPars : ColorAdjustmentPars
    {
        [DataMember(Name = "AvgWeightPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CMYKAvgWeightPars avgWeightPars;

        public CMYKAvgWeightPars AvgWeightPars
        {
            get { return avgWeightPars ?? (avgWeightPars = new CMYKAvgWeightPars()); }
        }

        public void Init(double[] source, ref int sourceIndex)
        {
            AvgWeightPars.Init(source, ref sourceIndex);
        }
    }
}
