using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public enum SCGScalingMethod : byte
    {
        Moller = 0, Safe = 1
    }
    
    public sealed class SCGRule : BackwardLearningRule
    {
        public SCGRule()
        {
            FaultTolerance = 1;
        }
        
        protected internal override LearningMode GetMode()
        {
            return LearningMode.Batch;
        }

        public override Type AlgorithmType
        {
            get { return typeof(SCGAlgorithm); }
        }

        [Required]
        [InitValue(SCGScalingMethod.Moller)]
        [DefaultValue(SCGScalingMethod.Moller)]
        [Category(PropertyCategories.Algorithm)]
        public SCGScalingMethod ScalingMethod { get; set; }

        [Required]
        [InitValue(1)]
        [DefaultValue(1)]
        [Category(PropertyCategories.Algorithm)]
        public int FaultTolerance { get; set; }
    }
}
