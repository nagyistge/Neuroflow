using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Core.Vectors
{
    public sealed class IterationRepeatPars
    {
        public IterationRepeatPars(
            [InitValue(1)]
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("Min Iterations")]
            int minIterations = 1,
            [InitValue(5)]
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("Max Iterations")]
            int maxIterations = 5)
        {
            Contract.Requires(minIterations >= 1);
            Contract.Requires(maxIterations >= minIterations);

            MinIterations = minIterations;
            MaxIterations = maxIterations;
        }
        
        public int MinIterations { get; private set; }

        public int MaxIterations { get; private set; }
    }
}
