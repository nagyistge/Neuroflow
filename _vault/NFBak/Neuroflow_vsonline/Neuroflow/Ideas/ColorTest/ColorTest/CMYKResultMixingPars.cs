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
    public sealed class CMYKResultMixingPars : ColorAdjustmentPars
    {
        [DataMember(Name = "CPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ResultMixingPars cPars;

        public ResultMixingPars CPars
        {
            get { return cPars ?? (cPars = new ResultMixingPars()); }
        }

        [DataMember(Name = "MPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ResultMixingPars mPars;

        public ResultMixingPars MPars
        {
            get { return mPars ?? (mPars = new ResultMixingPars()); }
        }

        [DataMember(Name = "YPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ResultMixingPars yPars;

        public ResultMixingPars YPars
        {
            get { return yPars ?? (yPars = new ResultMixingPars()); }
        }

        [DataMember(Name = "KPars", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ResultMixingPars kPars;

        public ResultMixingPars KPars
        {
            get { return kPars ?? (kPars = new ResultMixingPars()); }
        }

        public void InitPars(double[] source, ref int sourceIndex)
        {
            InitPars(CPars, source, ref sourceIndex);
            InitPars(MPars, source, ref sourceIndex);
            InitPars(YPars, source, ref sourceIndex);
            InitPars(KPars, source, ref sourceIndex);
        }

        void InitPars(ResultMixingPars pars, double[] source, ref int sourceIndex)
        {
            pars.PowValues = ResultMixingPars.CreateCMYKPowValues(source[sourceIndex++], source[sourceIndex++], source[sourceIndex++], source[sourceIndex++]);
            pars.MulValues = ResultMixingPars.CreateCMYKMulValues(source[sourceIndex++], source[sourceIndex++], source[sourceIndex++], source[sourceIndex++]);
            pars.Ratios = ResultMixingPars.CreateCMYKRatios(source[sourceIndex++], source[sourceIndex++], source[sourceIndex++], source[sourceIndex++]);
        }
    }
}
