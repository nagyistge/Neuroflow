using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    internal static class Consts
    {
        #region Constants

        internal const double Epsilon = 0.000001;

        internal const double MaxStdDev = 2.0;

        internal static readonly DoubleRange MeanRange = new DoubleRange(-1.0, 1.0);

        #endregion
    }
}
