using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Learning
{
    internal sealed class ErrorVectorStack
    {
        internal ErrorVectorStack(int maxSize, int errorVectorLength)
        {
            Contract.Requires(maxSize > 0);
            Contract.Requires(errorVectorLength > 0);

            errorVectors = new float[maxSize][];
            hasVectors = new bool[maxSize];
            for (int idx = 0; idx < maxSize; idx++)
            {
                errorVectors[idx] = new float[errorVectorLength];
            }
            index = -1;
        }

        float[][] errorVectors;

        bool[] hasVectors;

        int index;

        internal int MaxSize
        {
            get { return errorVectors.Length; }
        }

        internal int Size
        {
            get { return index + 1; }
        }

        internal void Push(float[] errors)
        {
            Contract.Requires(errors != null);
            Contract.Requires(errors.Length > 0);

            IncIdx();

            var v = errorVectors[index];
            for (int idx = 0; idx < v.Length; idx++) v[idx] = errors[idx];
            hasVectors[index] = true;
        }

        internal void PushNull()
        {
            IncIdx();
            hasVectors[index] = false;
        }

        internal float[] Pop()
        {
            if (index < 0) throw new InvalidOperationException("Cannot pop from stack, no values present.");

            float[] result;
            if (hasVectors[index])
            {
                result = errorVectors[index];
            }
            else
            {
                result = null;
            }
            index--;

            return result;
        }

        private void IncIdx()
        {
            if (index == MaxSize - 1) throw new InvalidOperationException("Cannot push to stack because it is full.");
            index++;
        }
    }
}
