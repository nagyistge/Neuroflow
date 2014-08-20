using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural.Learning;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class LearningLayer : Layer, IHasLearningRules
    {
        protected LearningLayer(int size, params LearningRule[] learningRules)
            : base(size)
        {
            Contract.Requires(size > 0);

            LearningRules = Array.AsReadOnly(learningRules);
        }

        [Browsable(false)]
        public ReadOnlyCollection<LearningRule> LearningRules { get; private set; }

        public bool NeedsForwardIteration
        {
            get { return LearningRules.Any(r => r.IsBeforeIterationRule); }
        }

        public bool NeedsGradientInformation
        {
            get { return LearningRules.Any(r => r.NeedsGradientInformation); }
        }
    }
}
