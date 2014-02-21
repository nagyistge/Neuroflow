using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core
{
    public interface IIdentified
    {
        Guid UID { get; }
    }
}
