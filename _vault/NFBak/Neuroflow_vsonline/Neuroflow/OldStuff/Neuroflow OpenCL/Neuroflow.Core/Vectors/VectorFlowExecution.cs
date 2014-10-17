using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using Neuroflow.Core.Computations;

namespace Neuroflow.Core.Vectors
{
    [Serializable]
    public abstract class VectorFlowExecution<T> : ISynchronized
        where T : struct
    {
        #region Constructor

        protected VectorFlowExecution(IComputationUnit<T> computationUnit)
        {
            Contract.Requires(computationUnit != null);

            ComputationUnit = computationUnit;
        } 

        #endregion

        #region Fields and Properties

        public IComputationUnit<T> ComputationUnit { get; private set; }

        public SyncContext SyncRoot
        {
            get { return ComputationUnit.SyncRoot; }
        }

        #endregion

        #region Execute

        public double ExecuteVectorFlow(VectorFlow<T> vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            lock (SyncRoot)
            {
                return LockedExecuteVectorFlow(vectorFlow);
            }
        }

        protected virtual double LockedExecuteVectorFlow(VectorFlow<T> vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            double errorSum = 0.0;
            double errorCount = 0.0;
            int entryCount = vectorFlow.entries.Length;

            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++) 
            {
                var entry = vectorFlow.entries[entryIndex];

                if (entry.InputVector != null) ComputationUnit.WriteInput(entry.InputVector); 

                for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations; iterationIndex++) Iteration();

                if (entry.DesiredOutputVector != null)
                {
                    for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations - 1; iterationIndex++) NoErrorAtCurrentIteration();
                    double? error = ComputeError(entry.DesiredOutputVector, entryIndex, entryCount);
                    if (error != null)
                    {
                        errorSum += error.Value;
                        errorCount++;
                    }
                    else
                    {
                        NoErrorAtCurrentIteration();
                    }
                }
                else
                {
                    for (int iterationIndex = 0; iterationIndex < entry.NumberOfIterations; iterationIndex++) NoErrorAtCurrentIteration();
                }
            }

            return errorCount != 0.0 ? errorSum / errorCount : 0.0;
        }
        
        protected unsafe abstract double? ComputeError(T?[] desiredOutputVector, int entryIndex, int entryCount);

        protected virtual void NoErrorAtCurrentIteration() { }

        protected virtual void Iteration()
        {
            ComputationUnit.Iteration();
        }

        #endregion
    }
}
