using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public class NeuralConnection : ComputationConnection<double>, IResetableNeuralNetworkItem
    {
        protected internal virtual void InitializeBackward(bool isRecurrent, int? errorValueIndex)
        {
            CheckIsInitialized();

            IsRecurrent = isRecurrent;

            if (errorValueIndex == null)
            {
                errorValue = new ValueReference<double>(ValueSpace);
            }
            else
            {
                errorValue = new ValueReference<double>(ValueSpace, errorValueIndex.Value);
            }

            gradientValue = new ValueReference<double>(ValueSpace);
            gradientSumValue = new ValueReference<double>(ValueSpace);

            if (isRecurrent)
            {
                recurrentInputValue = new ValueReference<double>(ValueSpace);
                savedInputValues = new Stack<double>();
            }
        }

        public bool IsRecurrent { get; private set; }

        public bool IsBackwardInitialized
        {
            get { return errorValue != null; }
        }

        Stack<double> savedInputValues;

        ValueReference<double> errorValue;

        public ValueReference<double> ErrorValue
        {
            get
            {
                CheckIsBacwardInitialized();
                return errorValue;
            }
        }

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

        ValueReference<double> recurrentInputValue;

        public ValueReference<double> RecurrentInputValue
        {
            get
            {
                CheckIsBacwardInitialized();
                return recurrentInputValue;
            }
        }

        internal string CreateInputValueExpression()
        {
            if (IsRecurrent)
            {
                return recurrentInputValue.Ref;
            }
            else
            {
                return InputValue.Ref;
            }
        }

        public virtual IEnumerable<int> GetResetErrorAffectedIndexes()
        {
            if (IsBackwardInitialized)
            {
                yield return errorValue.ValueIndex;
            }
        }

        public virtual IEnumerable<int> GetResetGradientAffectedIndexes()
        {
            if (IsBackwardInitialized)
            {
                yield return gradientValue.ValueIndex;
            }
        }

        public virtual IEnumerable<int> GetResetGradientSumAffectedIndexes()
        {
            if (IsBackwardInitialized)
            {
                yield return gradientSumValue.ValueIndex;
            }
        }

        protected internal virtual string CreateErrorValueExpression()
        {
            return ErrorValue.Ref;
        }

        protected internal virtual void DefinePushForwardInformation(ComputationBlock block, string theStack)
        {
            Contract.Requires(block != null);
            Contract.Requires(!String.IsNullOrEmpty(theStack));

            block.Add(theStack + ".Push(" + inputValue.Ref + ")");
        }

        protected internal virtual void DefinePopForwardInformation(ComputationBlock block, string theStack)
        {
            Contract.Requires(block != null);
            Contract.Requires(!String.IsNullOrEmpty(theStack));

            block.Add(recurrentInputValue.Ref + "=" + theStack + ".Pop()");
        }

        protected void CheckIsBacwardInitialized()
        {
            CheckIsInitialized();
            if (!IsBackwardInitialized) throw new InvalidOperationException("Neural connection backward phase is not initialized.");
        }
    }
}
