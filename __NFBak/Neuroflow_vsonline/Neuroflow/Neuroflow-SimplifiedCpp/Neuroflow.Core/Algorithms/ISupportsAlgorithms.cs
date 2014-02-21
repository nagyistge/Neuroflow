using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core.Algorithms
{
    public interface ISupportsAlgorithms
    {
        T GetAlgorithm<T>() where T : class;
    }
}
