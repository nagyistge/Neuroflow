using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public class InputLayer : Layer
    {
        public InputLayer(int size)
            : base(size)
        {
            Contract.Requires(size > 0);
        }
    }
}
