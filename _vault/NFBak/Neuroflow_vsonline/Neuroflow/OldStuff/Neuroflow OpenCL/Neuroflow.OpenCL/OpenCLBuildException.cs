using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Neuroflow.OpenCL
{
    public class OpenCLBuildException : OpenCLException
    {
        internal OpenCLBuildException(ProgramBuildLog[] logs, Exception inner)
            : base(inner.Message, inner)
        {
            Contract.Requires(logs != null);
            Contract.Requires(inner != null);

            Data["Logs"] = Array.AsReadOnly(logs);
        }

        public ReadOnlyCollection<ProgramBuildLog> Logs
        {
            get { return (ReadOnlyCollection<ProgramBuildLog>)Data["Logs"]; }
        }
    }
}
