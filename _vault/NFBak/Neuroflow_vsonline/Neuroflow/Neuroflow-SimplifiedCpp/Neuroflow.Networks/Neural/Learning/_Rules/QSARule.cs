using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public enum DistributionType
    {
        Uniform,
        Gaussian
    }
    
    public sealed class QSARule : ErrorBasedLearningRule
    {
        #region Defs

        const double DefUp = 0.95;

        const double DefDown = 0.01;

        const DistributionType DefDistributionType = DistributionType.Gaussian;

        #endregion

        #region Construct

        public QSARule()
        {
            Up = DefUp;
            Down = DefDown;
        }

        #endregion

        #region Props

        [Required]
        [InitValue(DefUp)]
        [DefaultValue(DefUp)]
        [Category(PropertyCategories.Math)]
        public double Up { get; set; }

        [Required]
        [InitValue(DefDown)]
        [DefaultValue(DefDown)]
        [Category(PropertyCategories.Math)]
        public double Down { get; set; }

        [Required]
        [InitValue(DefDistributionType)]
        [DefaultValue(DefDistributionType)]
        [Category(PropertyCategories.Math)]
        public DistributionType DistributionType { get; set; }

        #endregion

        #region Overrides

        protected internal override LearningMode GetMode()
        {
            return LearningMode.Batch;
        }

        public override bool NeedsGradientInformation
        {
            get { return false; }
        }

        #endregion
    }
}
