using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Computations
{
    public interface IComputationTestResult
    {
        double MSE { get; }
        
        IEnumerable<IEnumerable<double>> GetErrors();
    }
}
