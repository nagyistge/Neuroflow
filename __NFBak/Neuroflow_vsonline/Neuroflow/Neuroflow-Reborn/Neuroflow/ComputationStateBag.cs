using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal sealed class ComputationStateBag : IndexedResourceBag<IDisposable>
    {
        internal ComputationStateBag(IComputationStateFactory factory) :
            base(() => factory.CreateComputationState())
        {
            Debug.Assert(factory != null);
        }
    }
}
