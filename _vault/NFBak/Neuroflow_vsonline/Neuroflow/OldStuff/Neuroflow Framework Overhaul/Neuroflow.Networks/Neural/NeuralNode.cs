using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public abstract class NeuralNode : ComputationNode<double>, IResetableNeuralNetworkItem
    {
        protected internal virtual void InitializeBackward(bool isRecurrent, ValueReference<double> valueOfOneReference) 
        {
            CheckIsInitialized();

            IsBackwardInitialized = true;
            IsRecurrent = isRecurrent;
        }

        public bool IsRecurrent { get; private set; }

        public bool IsBackwardInitialized { get; private set; }

        protected void CheckIsBacwardInitialized()
        {
            CheckIsInitialized();
            if (!IsBackwardInitialized) throw new InvalidOperationException("Neural connection backward phase is not initialized.");
        }

        protected internal virtual ComputationBlock GetBackwardComputationBlock(BackwardComputationMode mode)
        {
            return null;
        }

        public virtual IEnumerable<int> GetResetErrorAffectedIndexes()
        {
            yield break;
        }

        public virtual IEnumerable<int> GetResetGradientAffectedIndexes()
        {
            yield break;
        }

        public virtual IEnumerable<int> GetResetGradientSumAffectedIndexes()
        {
            yield break;
        }

        protected internal virtual void DefinePushForwardInformation(ComputationBlock block, string theStack)
        {
            Contract.Requires(block != null);
            Contract.Requires(!String.IsNullOrEmpty(theStack));

            Debug.Assert(IsRecurrent);

            int l = upperConnectionArray.Length;
            for (int i = 0; i < l; i++)
            {
                ((NeuralConnection)upperConnectionArray[i]).DefinePushForwardInformation(block, theStack);
            }
        }

        protected internal virtual void DefinePopForwardInformation(ComputationBlock block, string theStack)
        {
            Contract.Requires(block != null);
            Contract.Requires(!String.IsNullOrEmpty(theStack));

            Debug.Assert(IsRecurrent);

            int l = upperConnectionArray.Length;
            for (int i = l-1; i >= 0; i--)
            {
                ((NeuralConnection)upperConnectionArray[i]).DefinePopForwardInformation(block, theStack);
            }
        }

        //protected internal virtual void PushForwardInformation() 
        //{
        //    Debug.Assert(IsRecurrent);

        //    int l = upperConnectionArray.Length;
        //    for (int i = 0; i < l; i++)
        //    {
        //        ((NeuralConnection)upperConnectionArray[i]).PushForwardInformation();
        //    }
        //}

        //protected internal virtual void PopForwardInformation()
        //{
        //    Debug.Assert(IsRecurrent);

        //    int l = upperConnectionArray.Length;
        //    for (int i = 0; i < l; i++)
        //    {
        //        ((NeuralConnection)upperConnectionArray[i]).PopForwardInformation();
        //    }
        //}
    }
}
