using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public enum LearningMode : byte { Stochastic, Batch }
    
    public abstract class ErrorBasedLearningRule : LearningRule
    {
        public override bool IsBeforeIterationRule
        {
            get { return false; }
        }
        
        protected internal abstract LearningMode GetMode();

        public sealed override bool IsErrorBasedRule
        {
            get { return true; }
        }

        [Category(PropertyCategories.Math)]
        public WeightDecay WeightDecay { get; set; }

        [Category(PropertyCategories.Algorithm)]
        public int IterationRepeat { get; set; }
    }
}
