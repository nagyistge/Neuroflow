using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public abstract class MultimodeGradientRule : GradientRule
    {
        public MultimodeGradientRule()
        {
            Mode = LearningMode.Batch;
        }
        
        public LearningMode Mode { get; set; }

        protected internal override LearningMode GetMode()
        {
            return Mode;
        }
    }
}
