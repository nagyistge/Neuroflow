using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public sealed class CMYKSplineValues : ColorAdjustmentPars
    {
        public CMYKSplineValues(int pointCount, bool reversible)
        {
            Contract.Requires(pointCount > 0);

            PointCount = pointCount;
            Reversible = reversible;
        }

        public int PointCount { get; private set; }

        public bool Reversible { get; private set; }

        private SplineValues cValues;

        public SplineValues CValues
        {
            get { return cValues ?? (cValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { cValues = value; }
        }

        private SplineValues mValues;

        public SplineValues MValues
        {
            get { return mValues ?? (mValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { mValues = value; }
        }

        private SplineValues yValues;

        public SplineValues YValues
        {
            get { return yValues ?? (yValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { yValues = value; }
        }

        private SplineValues kValues;

        public SplineValues KValues
        {
            get { return kValues ?? (kValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { kValues = value; }
        }

        public void Init(double[] values, ref int valueIndex)
        {
            CValues.Init(values, ref valueIndex);
            MValues.Init(values, ref valueIndex);
            YValues.Init(values, ref valueIndex);
            KValues.Init(values, ref valueIndex);
        }
    }
}
