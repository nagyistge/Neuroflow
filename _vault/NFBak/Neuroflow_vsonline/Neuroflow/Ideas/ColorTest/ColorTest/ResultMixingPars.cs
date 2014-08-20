using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;

namespace ColorTest
{
    [Serializable, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class ResultMixingPars : ColorAdjustmentPars
    {
        [DataMember(Name = "PowValues", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CMYKAmmountValues powValues;

        public CMYKAmmountValues PowValues
        {
            get { return powValues ?? (powValues = CreateDefaultCMYKPowValues()); }
            set
            {
                Contract.Requires(value != null);

                powValues = value;
            }
        }

        [DataMember(Name = "MulValues", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CMYKAmmountValues mulValues;

        public CMYKAmmountValues MulValues
        {
            get { return mulValues ?? (mulValues = CreateDefaultCMYKMulValues()); }
            set
            {
                Contract.Requires(value != null);

                mulValues = value;
            }
        }

        [DataMember(Name = "Ratios", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CMYKRatioValues ratios;

        public CMYKRatioValues Ratios
        {
            get { return ratios ?? (ratios = CreateDefaultCMYKRatios()); }
            set
            {
                Contract.Requires(value != null);

                ratios = value;
            }
        }
    }
}
