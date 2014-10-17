using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using NeoComp.Core;

namespace NeoComp.Computations2
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compScriptExec")]
    [ContractClass(typeof(ComputationScriptExecutionContract<>))]
    public abstract class ComputationScriptExecution<T>
        where T : struct
    {
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
                if (taken) Monitor.Exit(sync);
            }
        }
        
        protected ComputationScriptExecution(IComputationUnit<T> computationUnit)
        {
            Contract.Requires(computationUnit != null);

            ComputationUnit = computationUnit;
        }
        
        [DataMember(Name = "unit")]
        public IComputationUnit<T> ComputationUnit { get; private set; }

        protected virtual double Excute(IEnumerable<ComputationScriptEntry<T>> scriptEntries)
        {
            Contract.Requires(scriptEntries != null);

            double errorSum = 0.0;
            double errorCount = 0.0;
            using (new ComputationUnitGuard(this))
            {
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

        protected abstract double ComputeError(T?[] desiredOutputVector);

        protected virtual void Iterate(int numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);

            for (int idx = 0; idx < numberOfIterations; idx++) ComputationUnit.Iteration();
        }

        protected virtual void WriteInputVector(T?[] inputVector)
        {
            Contract.Requires(inputVector != null && inputVector.Length == ComputationUnit.InputInterface.Length); 
            
            ComputationUnit.InputInterface.Write(inputVector);
        }
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
