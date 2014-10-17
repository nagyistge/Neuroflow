using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class GaussianHistoryRule : ErrorBasedLearningRule
    {
        #region Construct

        public GaussianHistoryRule()
        {
            StdDev = DefStdDev;
            Size = DefSize;
            Scale = DefScale;
        }

        #endregion

        #region Defs

        const double DefStdDev = 0.3;

        const int DefSize = 10;

        const double DefScale = 1.0;

        #endregion

        #region Overrides

        public override bool NeedsGradientInformation
        {
            get { return false; }
        }

        protected internal override LearningMode GetMode()
        {
            return LearningMode.Batch;
        }

        #endregion

        #region Properties

        [Required]
        [InitValue(DefStdDev)]
        [DefaultValue(DefStdDev)]
        [Category(PropertyCategories.Math)]
        public double StdDev { get; set; }

        [Required]
        [InitValue(DefSize)]
        [DefaultValue(DefSize)]
        [Category(PropertyCategories.Algorithm)]
        public int Size { get; set; }

        [Required]
        [InitValue(DefScale)]
        [DefaultValue(DefScale)]
        [Category(PropertyCategories.Math)]
        public double Scale { get; set; }

        #endregion
    }
}
