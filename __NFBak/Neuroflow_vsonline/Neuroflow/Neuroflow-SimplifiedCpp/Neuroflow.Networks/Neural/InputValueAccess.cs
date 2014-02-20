using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public struct InputValueAccess
    {
        public InputValueAccess(int inputSize, int inputBufferBeginIndex, int weightBufferBeginIndex, IntRange[] innerItarationInputValueStack)
        {
            Contract.Requires(inputSize > 0);
            Contract.Requires(inputBufferBeginIndex >= 0);
            Contract.Requires(weightBufferBeginIndex >= 0);

            this.inputSize = inputSize;
            this.inputBufferBeginIndex = inputBufferBeginIndex;
            this.weightBufferBeginIndex = weightBufferBeginIndex;
            this.innerItarationInputValueStack = innerItarationInputValueStack;
        }

        int inputSize;

        public int InputSize
        {
            get { return inputSize; }
        }

        int inputBufferBeginIndex;

        public int InputBufferBeginIndex
        {
            get { return inputBufferBeginIndex; }
        }

        int weightBufferBeginIndex;

        public int WeightBufferBeginIndex
        {
            get { return weightBufferBeginIndex; }
        }

        IntRange[] innerItarationInputValueStack;

        public IntRange[] InnerItarationInputValueStack
        {
            get { return innerItarationInputValueStack; }
        }
    }
}
