using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public sealed class RGBSplineValues : ColorAdjustmentPars
    {
        public RGBSplineValues(int pointCount, bool reversible)
        {
            Contract.Requires(pointCount > 0);

            PointCount = pointCount;
            Reversible = reversible;
        }

        public int PointCount { get; private set; }

        public bool Reversible { get; private set; }

        private SplineValues rValues;

        public SplineValues RValues
        {
            get { return rValues ?? (rValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { rValues = value; }
        }

        private SplineValues gValues;

        public SplineValues GValues
        {
            get { return gValues ?? (gValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { gValues = value; }
        }

        private SplineValues bValues;

        public SplineValues BValues
        {
            get { return bValues ?? (bValues = new SplineValues(PointCount, Reversible, SplineResolution)); }
            set { bValues = value; }
        }

        public void Init(double[] values, ref int valueIndex)
        {
            RValues.Init(values, ref valueIndex);
            GValues.Init(values, ref valueIndex);
            BValues.Init(values, ref valueIndex);
        }
    }
}
