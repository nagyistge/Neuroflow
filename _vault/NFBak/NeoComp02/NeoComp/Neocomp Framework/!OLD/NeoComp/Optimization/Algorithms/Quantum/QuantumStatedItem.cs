using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public class QuantumStatedItem : IQuantumStatedItem
    {
        public virtual QuantumState State { get; set; }
    }
}
