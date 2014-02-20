using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;

namespace ColorTest
{
    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class CMYKAvgWeightPars : ColorAdjustmentPars
    {
        [DataMember(Name = "CPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AvgWeightPars cPars;

        public AvgWeightPars CPars
        {
            get { return cPars ?? (cPars = new AvgWeightPars()); }
        }

        [DataMember(Name = "MPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AvgWeightPars mPars;

        public AvgWeightPars MPars
        {
            get { return mPars ?? (mPars = new AvgWeightPars()); }
        }

        [DataMember(Name = "YPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AvgWeightPars yPars;

        public AvgWeightPars YPars
        {
            get { return yPars ?? (yPars = new AvgWeightPars()); }
        }

        [DataMember(Name = "KPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AvgWeightPars kPars;

        public AvgWeightPars KPars
        {
            get { return kPars ?? (kPars = new AvgWeightPars()); }
        }

        public void Init(double[] values, ref int valueIndex)
        {
            CPars.Init(values, ref valueIndex);
            MPars.Init(values, ref valueIndex);
            YPars.Init(values, ref valueIndex);
            KPars.Init(values, ref valueIndex);
        }
    }
}
