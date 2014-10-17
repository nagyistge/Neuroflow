using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;

namespace Neuroflow.Core.Vectors
{
    [Serializable]
    public abstract class SupervisedExecution<T> 
        where T : struct
    {
        #region Constructor

        protected SupervisedExecution(IComputationUnit<T> computationUnit)
        {
            Contract.Requires(computationUnit != null);

            ComputationUnit = computationUnit;
        } 

        #endregion

        #region Fields and Properties

        public IComputationUnit<T> ComputationUnit { get; private set; }

        #endregion

        #region Execute

        public void ExecuteVectorFlow(VectorComputationContext context, VectorBuffer<T> vectorBuffer, VectorFlow<T> vectorFlow)
        {
            Contract.Requires(context != null);
            Contract.Requires(vectorBuffer != null);
            Contract.Requires(vectorFlow != null);

            lock (context.SyncRoot)
            {
                lock (vectorBuffer.SyncRoot)
                {
                    DoExecuteVectorFlow(context, vectorBuffer, vectorFlow);
                }
            }
        }

        protected virtual void DoExecuteVectorFlow(VectorComputationContext context, VectorBuffer<T> vectorBuffer, VectorFlow<T> vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            int entryCount = vectorFlow.entries.Length;

            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++) 
            {
                var entry = vectorFlow.entries[entryIndex];
                T[] input, desiredOutput;

                GetInputAndDesiredOutput(entry, out input, out desiredOutput);

                if (input != null)
                {
                    var inputVector = vectorBuffer.GetOrCreate(vectorFlow.Index, entryIndex * 2, () => input);
                    ComputationUnit.WriteInput(context, inputVector);
                }

                for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations; iterationIndex++) Iteration(context);

                if (desiredOutput != null)
                {
                    for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations - 1; iterationIndex++) NoErrorAtCurrentIteration(context);

                    var desiredOutputVector = vectorBuffer.GetOrCreate(vectorFlow.Index, entryIndex * 2 + 1, () => desiredOutput);
                    ComputeError(context, desiredOutputVector, entryIndex, entryCount);
                }
                else
                {
                    for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations; iterationIndex++) NoErrorAtCurrentIteration(context);
                }
            }
        }

        protected unsafe abstract void ComputeError(VectorComputationContext context, BufferedVector<T> desiredOutputVector, int entryIndex, int entryCount);

        protected virtual void NoErrorAtCurrentIteration(VectorComputationContext context) { }

        protected virtual void Iteration(VectorComputationContext context)
        {
            ComputationUnit.Iteration(context);
        }

        private void GetInputAndDesiredOutput(VectorFlowEntry<T> entry, out T[] input, out T[] desiredOutput)
        {
            input = entry.Vectors[0];
            desiredOutput = entry.Vectors.Length != 1 ? entry.Vectors[1] : null;
        }

        #endregion
    }
}
