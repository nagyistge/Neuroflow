using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.OpenCL
{
    public class OpenCLException : Exception
    {
        public OpenCLException(string message)
            : base(message)
        {
        }

        public OpenCLException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
