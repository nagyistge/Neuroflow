using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using Neuroflow.Core.ComputationAPI;

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

            foreach (var entry in vectorFlow.entries)
            {
                if (entry.InputVector != null) WriteInputVector(entry.InputVector);

                for (int idx = 0; idx < entry.NumberOfIterations; idx++) Iteration(idx + 1, entry.NumberOfIterations);
                
                if (entry.DesiredOutputVector != null)
                {
                    double? error = ComputeError(entry.DesiredOutputVector);
                    if (error != null)
                    {
                        errorSum += error.Value;
                        errorCount++;
                    }
                }
            }

            return errorCount != 0.0 ? errorSum / errorCount : 0.0;
        }

        protected abstract double? ComputeError(T?[] desiredOutputVector);

        protected virtual void Iteration(int iteration, int numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(iteration > 0);
            Contract.Requires(iteration <= numberOfIterations);

            ComputationUnit.Iteration();
        }

        unsafe protected virtual void WriteInputVector(T?[] inputVector)
        {
            Contract.Requires(inputVector != null && inputVector.Length == ComputationUnit.InputInterface.Length);

            ComputationUnit.InputInterface.Write(inputVector);
        } 

        #endregion
    }
}
