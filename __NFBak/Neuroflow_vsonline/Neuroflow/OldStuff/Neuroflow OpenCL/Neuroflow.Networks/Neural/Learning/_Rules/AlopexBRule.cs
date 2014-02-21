using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class AlopexBRule : ErrorBasedLearningRule
    {
        #region Defs

        const double DefEta = 0.01;

        const double DefGamma = 0.05;

        const double DefLambda = 0.5;

        #endregion

        #region Construct

        public AlopexBRule()
        {
            Eta = DefEta;
            Gamma = DefGamma;
            Lambda = DefLambda;
        }

        #endregion

        #region Type Def

        protected internal override LearningMode GetMode()
        {
            return LearningMode.Batch;
        }

        public override bool NeedsGradientInformation
        {
            get { return false; }
        } 

        #endregion

        #region Props

        [Required]
        [InitValue(DefEta)]
        [DefaultValue(DefEta)]
        [Category(PropertyCategories.Math)]
        public double Eta { get; set; }

        [Required]
        [InitValue(DefGamma)]
        [DefaultValue(DefGamma)]
        [Category(PropertyCategories.Math)]
        public double Gamma { get; set; }

        [Required]
        [InitValue(DefLambda)]
        [DefaultValue(DefLambda)]
        [Category(PropertyCategories.Math)]
        public double Lambda { get; set; }

        #endregion
    }
}
