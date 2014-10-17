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
    public sealed class AvgWeightPars : ColorAdjustmentPars
    {
        [DataMember(Name = "SplineValues", EmitDefaultValue = false)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CMYKSplineValues splineValues;

        public CMYKSplineValues SplineValues
        {
            get { return splineValues ?? (splineValues = new CMYKSplineValues(SplineControlPoints, false)); }
            set 
            {
                Contract.Requires(value != null);

                splineValues = value; 
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

        public void Init(double[] values, ref int valueIndex)
        {
            SplineValues.Init(values, ref valueIndex);
            Ratios = new CMYKRatioValues(true, values[valueIndex++], values[valueIndex++], values[valueIndex++], values[valueIndex++]);
        }
    }
}
