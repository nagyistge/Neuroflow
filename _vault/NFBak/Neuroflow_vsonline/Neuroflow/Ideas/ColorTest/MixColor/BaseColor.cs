using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ColorTest;

namespace MixColor
{
    public sealed class BaseColor
    {
        public BaseColor(string name, HdrRGB color)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            Name = name;
            Color = color;
        }

        public string Name { get; private set; }

        public HdrRGB Color { get; private set; }
    }
}
