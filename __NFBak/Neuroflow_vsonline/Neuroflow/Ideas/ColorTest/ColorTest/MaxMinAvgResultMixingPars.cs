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
    public sealed class MaxMinAvgResultMixingPars : ColorAdjustmentPars
    {
        [DataMember(Name = "AvgPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CMYKResultMixingPars avgPars;

        public CMYKResultMixingPars AvgPars
        {
            get { return avgPars ?? (avgPars = new CMYKResultMixingPars()); }
        }

        public void InitPars(double[] source, ref int sourceIndex)
        {
            InitPars(AvgPars, source, ref sourceIndex);
        }

        void InitPars(CMYKResultMixingPars pars, double[] source, ref int sourceIndex)
        {
            pars.InitPars(source, ref sourceIndex);
        }
    }
}
