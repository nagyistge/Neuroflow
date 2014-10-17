using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core
{
    public abstract class Identified : IIdentified
    {
        Guid uid = Guid.NewGuid();

        public Guid UID
        {
            get { return uid; }
            protected set { uid = value; }
        }
    }
}
