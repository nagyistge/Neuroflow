using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "itRepPars")]
    public sealed class IterationRepeatPars
    {
        public IterationRepeatPars(int iterations)
        {
            Contract.Requires(iterations >= 1);
            MinIterations = MaxIterations = iterations;
        }
        
        public IterationRepeatPars(int minIterations = 1, int maxIterations = 5)
        {
            Contract.Requires(minIterations >= 1);
            Contract.Requires(maxIterations >= minIterations);

            MinIterations = minIterations;
            MaxIterations = maxIterations;
        }
        
        [DataMember(Name = "minIts")]
        public int MinIterations { get; private set; }

        [DataMember(Name = "maxIts")]
        public int MaxIterations { get; private set; }
    }
}
