using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using NeoComp.Threading;
using NeoComp.Computations;

namespace NeoComp.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "vectorFlowExec")]
    [ContractClass(typeof(VectorFlowExecutionContract<>))]
    public abstract class VectorFlowExecution<T> : IReset
        where T : struct
    {
        #region Guard Class

        protected class ComputationUnitGuard : IDisposable
        {
            public ComputationUnitGuard(VectorFlowExecution<T> exec)
            {
                sync = exec.ComputationUnit as ISynchronized;
                if (sync != null) Monitor.Enter(sync.SyncRoot, ref taken);
            }

            ISynchronized sync;

            bool taken;

            public void Dispose()
            {
                if (taken) Monitor.Exit(sync.SyncRoot);
            }
        } 

        #endregion

        #region Constructor

        protected VectorFlowExecution(IComputationUnit<T> computationUnit)
        {
            Contract.Requires(computationUnit != null);

            ComputationUnit = computationUnit;
        } 

        #endregion

        #region Fields and Properties

        [NonSerialized]
        ResetHandler cuResetHandler;

        private ResetHandler CUResetHandler
        {
            get { return cuResetHandler ?? (cuResetHandler = new ResetHandler(ComputationUnit, false)); }
        }

        [DataMember(Name = "unit")]
        public IComputationUnit<T> ComputationUnit { get; private set; }

        #endregion

        #region Execute

        protected virtual double Excute(VectorFlow<T> vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            double errorSum = 0.0;
            double errorCount = 0.0;
            using (new ComputationUnitGuard(this))
            {
                // Compute:
                foreach (var entry in vectorFlow.ItemArray)
                {
                    if (entry != null)
                    {
                        if (entry.InputVector != null) WriteInputVector(entry.InputVector);
                        Iterate(entry.NumberOfIterations);
                        if (entry.DesiredOutputVector != null)
                        {
                            errorSum += ComputeError(entry.DesiredOutputVector);
                            errorCount++;
                        }
                    }
                }
            }

            return errorCount != 0.0 ? errorSum / errorCount : 0.0;
        }

        public void Reset()
        {
            using (new ComputationUnitGuard(this)) DoReset();
        }

        protected virtual void DoReset()
        {
            ResetComputationUnit();
        }

        protected void ResetComputationUnit()
        {
            if (!ComputationUnit.IsFeedForward) CUResetHandler.Reset();
        }

        protected abstract double ComputeError(T?[] desiredOutputVector);

        protected virtual void Iterate(int numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);

            for (int idx = 0; idx < numberOfIterations; idx++) Iteration(idx + 1, numberOfIterations);
        }

        protected virtual void Iteration(int iteration, int numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(iteration > 0);
            Contract.Requires(iteration <= numberOfIterations);

            ComputationUnit.Iteration();
        }

        protected virtual void WriteInputVector(T?[] inputVector)
        {
            Contract.Requires(inputVector != null && inputVector.Length == ComputationUnit.InputInterface.Length);

            ComputationUnit.InputInterface.Write(inputVector);
        } 

        #endregion
    }

    [ContractClassFor(typeof(VectorFlowExecution<>))]
    abstract class VectorFlowExecutionContract<T> : VectorFlowExecution<T>
        where T : struct
    {
        protected VectorFlowExecutionContract() : base(null) { }
        
        protected override double ComputeError(T?[] desiredOutputVector)
        {
            Contract.Requires(desiredOutputVector != null && desiredOutputVector.Length == ComputationUnit.OutputInterface.Length);
            return 0;
        }
    }
}
