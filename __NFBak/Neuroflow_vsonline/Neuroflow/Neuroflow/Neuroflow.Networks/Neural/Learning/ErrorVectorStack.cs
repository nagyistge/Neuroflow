using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Core.Vectors;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning
{
    internal sealed class ErrorVectorStack
    {
        internal ErrorVectorStack(NeuralNetwork network, BufferAllocator allocator)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);

            this.network = network;
            int maxSize = network.RecurrentOptions.MaxIterations;
            int errorVectorLength = network.OutputInterfaceLength;

            errorVectors = new IntRange[maxSize];
            for (int idx = 0; idx < maxSize; idx++)
            {
                errorVectors[idx] = allocator.Alloc(errorVectorLength);
            }
        }

        NeuralNetwork network;

        IntRange[] errorVectors;

        internal int MaxSize
        {
            get { return errorVectors.Length; }
        }

        internal int GetSize(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            lock (context.SyncRoot)
            {
                return context.ErrorVectorStack_Index + 1;
            }
        }

        internal void Initialize(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            context.ErrorVectorStack_Index = -1;
            context.ErrorVectorStack_HasVectors = new bool[MaxSize];
        }

        internal void Push(NeuralComputationContext context, IntRange errors)
        {
            Contract.Requires(context != null);

            IncIdx(context);

            var v = errorVectors[context.ErrorVectorStack_Index];
            network.CopyBuffer(context, errors, v);
            context.ErrorVectorStack_HasVectors[context.ErrorVectorStack_Index] = true;
        }

        internal void PushNull(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            IncIdx(context);
            context.ErrorVectorStack_HasVectors[context.ErrorVectorStack_Index] = false;
        }

        internal IntRange? Pop(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            if (context.ErrorVectorStack_Index < 0) throw new InvalidOperationException("Cannot pop from stack, no values present.");

            IntRange? result;
            if (context.ErrorVectorStack_HasVectors[context.ErrorVectorStack_Index])
            {
                result = errorVectors[context.ErrorVectorStack_Index];
            }
            else
            {
                result = null;
            }
            context.ErrorVectorStack_Index--;

            return result;
        }

        private void IncIdx(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            if (context.ErrorVectorStack_Index == MaxSize - 1) throw new InvalidOperationException("Cannot push to stack because it is full.");
            context.ErrorVectorStack_Index++;
        }
    }
}
