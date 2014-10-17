using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gender.Data
{
    internal static class Helpers
    {
        internal static double PixelToDouble(byte pixel)
        {
            return ((double)pixel / 255.0) * 2.0 - 1.0;
        }
    }
}
