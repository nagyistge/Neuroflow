using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public enum RLAlgorithm : byte
    {
        BPTT = 0, RTLR = 1
    }

    public sealed class RecurrentOptions
    {
        public RecurrentOptions(
            [FreeDisplayName("Max Iterations")]
            [Required]
            [InitValue(1)]
            [Category(PropertyCategories.Structure)]
            int maxIterations, 
            [FreeDisplayName("Is Full Recurrent")]
            [InitValue(false)]
            [Required]
            [Category(PropertyCategories.Structure)]
            bool isFullRecurrent = false,
            [FreeDisplayName("Algorithm")]
            [InitValue(RLAlgorithm.BPTT)]
            [Required]
            [Category(PropertyCategories.Algorithm)]
            RLAlgorithm algorithm = RLAlgorithm.BPTT)
        {
            Contract.Requires(maxIterations > 0);

            MaxIterations = maxIterations;
            IsFullRecurrent = isFullRecurrent;
            Algorithm = algorithm;
        }
        
        public int MaxIterations { get; private set; }

        public bool IsFullRecurrent { get; private set; }

        public RLAlgorithm Algorithm { get; private set; }
    }
}
