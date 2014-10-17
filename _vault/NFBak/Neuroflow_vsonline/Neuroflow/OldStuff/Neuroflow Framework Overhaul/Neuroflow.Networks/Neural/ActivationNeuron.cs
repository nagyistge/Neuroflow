using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural
{
    public class ActivationNeuron : NeuralNode, IHasLearningRules
    {
        public ActivationNeuron(
            [Required]
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("ActivationFunction")]
            ActivationFunction activationFunction,
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("LearningRules")]
            params LearningRule[] learningRules)
        {
            Contract.Requires(activationFunction != null);

            ActivationFunction = activationFunction;
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
                    InputValueIndex = valueOfOneReference.ValueIndex,
                    WeightValueIndex = biasValue.ValueIndex
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

        #region Reset

        public override IEnumerable<int> GetResetGradientAffectedIndexes()
        {
            if (IsBackwardInitialized)
            {
                yield return gradientValue.ValueIndex;
            }
        }

        public override IEnumerable<int> GetResetGradientSumAffectedIndexes()
        {
            if (IsBackwardInitialized)
            {
                yield return gradientSumValue.ValueIndex;
            }
        }

        #endregion

        #region Forward

        public ActivationFunction ActivationFunction { get; private set; }

        ValueReference<double> biasValue;

        public ValueReference<double> BiasValue
        {
            get
            {
                CheckIsInitialized();
                return biasValue;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            biasValue = new ValueReference<double>(ValueSpace);
        }

        protected override ComputationBlock CreateComputationBlock()
        {
            var block = new ComputationBlock();
            string sum = biasValue.Ref;
            foreach (var conn in this.UpperConnections)
            {
                sum = sum + "+(" + conn.CreateComputationExpression() + ")";
            }
            block.Add("double anSum=" + sum);

            if (IsBackwardInitialized)
            {
                block.Add("double anResult=" + ActivationFunction.Function(block, "anSum"));
                string result = "anResult";
                block.Add(LowerConnections[0].InputValue.Ref + "=" + result);
                if (IsRecurrent)
                {
                    Debug.Assert(derivateValue != null);

                    block.Add(derivateValue.Ref + "=" + ActivationFunction.Derivate(block, result));
                }
            }
            else
            {
                block.Add(LowerConnections[0].InputValue.Ref + "=" + ActivationFunction.Function(block, "anSum"));
            }

            return block;
        } 

        #endregion

        #region Backward

        protected internal override void InitializeBackward(bool isRecurrent, ValueReference<double> valueOfOneReference)
        {
            base.InitializeBackward(isRecurrent, valueOfOneReference);

            gradientValue = new ValueReference<double>(ValueSpace);
            gradientSumValue = new ValueReference<double>(ValueSpace);
            this.valueOfOneReference = valueOfOneReference;

            if (isRecurrent)
            {
                savedDerivates = new Stack<double>();
                derivateValue = new ValueReference<double>(ValueSpace);
            }
        }

        Stack<double> savedDerivates;

        ValueReference<double> derivateValue;

        ValueReference<double> valueOfOneReference;

        ValueReference<double> gradientValue;

        public ValueReference<double> GradientValue
        {
            get
            {
                CheckIsBacwardInitialized();
                return gradientValue;
            }
        }

        ValueReference<double> gradientSumValue;

        public ValueReference<double> GradientSumValue
        {
            get
            {
                CheckIsBacwardInitialized();
                return gradientSumValue;
            }
        }

        protected internal override void DefinePushForwardInformation(ComputationBlock block, string theStack)
        {
            base.DefinePushForwardInformation(block, theStack); // Push Inputs and Co.

            block.Add(theStack + ".Push(" + derivateValue.Ref + ")");
        }

        protected internal override void DefinePopForwardInformation(ComputationBlock block, string theStack)
        {
            block.Add(derivateValue.Ref + "=" + theStack + ".Pop()"); 
            
            base.DefinePopForwardInformation(block, theStack); // Pop Inputs and Co.
        }

        protected internal override ComputationBlock GetBackwardComputationBlock(BackwardComputationMode mode)
        {
            var block = new ComputationBlock();

            string derivate;

            if (IsRecurrent)
            {
                derivate = derivateValue.Ref;
            }
            else
            {
                string outputValue = LowerConnections[0].InputValue.Ref; // Because this is an op. node
                derivate = ActivationFunction.Derivate(block, outputValue);
            }

            var errorSb = new StringBuilder();
            foreach (NeuralConnection outputConn in LowerConnections)
            {
                if (errorSb.Length != 0) errorSb.Append("+");
                errorSb.Append("(" + outputConn.CreateErrorValueExpression() + ")");
            }

            string error = "((" + errorSb.ToString() + ")*(" + derivate + "))";

            block.Add("double bwError=" + error);

            // Set Bias:            
            if (mode == BackwardComputationMode.FeedForward)
            {
                block.Add(gradientValue.Ref + "=bwError");
                block.Add(gradientSumValue.Ref + "+=bwError");
            }
            else if (mode == BackwardComputationMode.Recurrent)
            {
                block.Add(gradientValue.Ref + "+=bwError");
            }
            else // BackwardComputationMode.RecurrentLastStep
            {
                block.Add(gradientValue.Ref + "+=bwError");
                block.Add(gradientSumValue.Ref + "+=" + gradientValue.Ref);
            }

            // Set Input Connections:
            block.Add("double igv");            
            foreach (NeuralConnection inputConn in UpperConnections)
            {
                // Set Error:
                block.Add(inputConn.ErrorValue.Ref + "=bwError");

                // Set Gradinets                
                block.Add("igv=(bwError*" + inputConn.CreateInputValueExpression() + ")");

                if (mode == BackwardComputationMode.FeedForward)
                {
                    block.Add(inputConn.GradientValue.Ref + "=igv");
                    block.Add(inputConn.GradientSumValue.Ref + "+=igv");
                }
                else if (mode == BackwardComputationMode.Recurrent)
                {
                    block.Add(inputConn.GradientValue.Ref + "+=igv");
                }
                else // BackwardComputationMode.RecurrentLastStep
                {
                    block.Add(inputConn.GradientValue.Ref + "+=igv");
                    block.Add(inputConn.GradientSumValue.Ref + "+=" + inputConn.GradientValue.Ref);
                }
            }

            return block;
        }

        #endregion
    }
}
