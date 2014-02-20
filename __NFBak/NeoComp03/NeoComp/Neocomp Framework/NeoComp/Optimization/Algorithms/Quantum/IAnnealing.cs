using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public interface IAnnealing
    {
        double CurrentEnergy { get; }
        
        void Reset();

        void Step();
    }
}
