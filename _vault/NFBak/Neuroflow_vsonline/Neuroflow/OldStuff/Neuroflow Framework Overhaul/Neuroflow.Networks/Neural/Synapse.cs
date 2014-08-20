using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Neuroflow.Networks.Neural
{
    public class Synapse : NeuralConnection, IHasLearningRules
    {
        public Synapse(
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("LearningRules")]
            params LearningRule[] learningRules)
        {
            this.learningRules = learningRules;
        }

        internal LearningRule[] learningRules;

        [Browsable(false)]
        public ReadOnlyCollection<LearningRule> LearningRules
        {
            get { return Array.AsReadOnly(learningRules); }
        }

        InputValueIndexes IHasLearningRules.InputValueIndexes
        {
            get
            {
                CheckIsInitialized();
                return new InputValueIndexes
                {
                    InputValueIndex = inputValue.ValueIndex,
                    WeightValueIndex = weightValue.ValueIndex
                };
            }
        }

        OutputValueIndexes IHasLearningRules.OutputValueIndex
        {
            get
            {
                CheckIsBacwardInitialized();
                return new OutputValueIndexes
                {
                    GradientValueIndex = GradientValue.ValueIndex,
                    GradientSumValueIndex = GradientSumValue.ValueIndex
                };
            }
        }

        ValueReference<double> weightValue;

        public ValueReference<double> WeightValue
        {
            get
            {
                CheckIsInitialized();
                return weightValue;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            weightValue = new ValueReference<double>(ValueSpace);
        }

        protected internal override string CreateComputationExpression()
        {
            return InputValue.Ref + "*" + WeightValue.Ref;
        }

        protected internal override string CreateErrorValueExpression()
        {
            return ErrorValue.Ref + "*" + WeightValue.Ref;
        }
    }
}
