using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public enum ResetMode : byte { Reinitialize, NewExecution }
    
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compScriptExec")]
    [ContractClass(typeof(ComputationScriptExecutionContract<>))]
    public abstract class ComputationScriptExecution<T> : IReset
        where T : struct
    {
        #region Guard Class

        protected class ComputationUnitGuard : IDisposable
        {
            public ComputationUnitGuard(ComputationScriptExecution<T> exec)
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

        protected ComputationScriptExecution(IComputationUnit<T> computationUnit)
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

        protected virtual double Excute(IEnumerable<ComputationScriptEntry<T>> scriptEntries)
        {
            Contract.Requires(scriptEntries != null);

            double errorSum = 0.0;
            double errorCount = 0.0;
            using (new ComputationUnitGuard(this))
            {
                // Reset for new script execution:
                Reset(ResetMode.NewExecution);

                // Compute:
                foreach (var entry in scriptEntries)
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
            using (new ComputationUnitGuard(this)) Reset(ResetMode.Reinitialize);
        }

        protected virtual void Reset(ResetMode mode)
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

    [ContractClassFor(typeof(ComputationScriptExecution<>))]
    abstract class ComputationScriptExecutionContract<T> : ComputationScriptExecution<T>
        where T : struct
    {
        protected ComputationScriptExecutionContract() : base(null) { }
        
        protected override double ComputeError(T?[] desiredOutputVector)
        {
            Contract.Requires(desiredOutputVector != null && desiredOutputVector.Length == ComputationUnit.OutputInterface.Length);
            return 0;
        }
    }
}
