using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public class Computation<T>
    {
        public Computation(int numberOfIterations = 1)
        {
            Contract.Requires(numberOfIterations > 0);
            
            NumberOfIterations = numberOfIterations;
        }

        public int NumberOfIterations { get; private set; }

        public T[] Compute(IComputationalUnit<T> compUnit, T[] inputBuffer, CancellationToken? cancellationToken = null)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputBuffer != null);
            Contract.Requires(inputBuffer.Length == compUnit.InputInterface.Length);
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == compUnit.OutputInterface.Length); 
            
            var outputBuffer = new T[compUnit.OutputInterface.Length];
            ComputeInternal(compUnit, inputBuffer, outputBuffer, cancellationToken);
            return outputBuffer;
        }

        public void Compute(IComputationalUnit<T> compUnit, T[] inputBuffer, T[] outputBuffer, CancellationToken? cancellationToken = null)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputBuffer != null);
            Contract.Requires(outputBuffer != null);
            Contract.Requires(inputBuffer.Length == compUnit.InputInterface.Length);
            Contract.Requires(outputBuffer.Length == compUnit.OutputInterface.Length);

            ComputeInternal(compUnit, inputBuffer, outputBuffer, cancellationToken);
        }

        internal void ComputeInternal(IComputationalUnit<T> compUnit, T[] inputBuffer, T[] outputBuffer, CancellationToken? cancellationToken)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputBuffer != null);
            Contract.Requires(outputBuffer != null);
            Contract.Requires(inputBuffer.Length == compUnit.InputInterface.Length);
            Contract.Requires(outputBuffer.Length == compUnit.OutputInterface.Length);

            var sync = compUnit as ISynchronized;
            bool lockTaken = false;
            try
            {
                if (sync != null) Monitor.Enter(sync.SyncRoot, ref lockTaken);

                SafeComputeInternal(compUnit, inputBuffer, outputBuffer, cancellationToken);
            }
            finally
            {
                if (sync != null && lockTaken) Monitor.Exit(sync.SyncRoot);
            }
        }

        internal void SafeComputeInternal(IComputationalUnit<T> compUnit, T[] inputBuffer, T[] outputBuffer, CancellationToken? cancellationToken)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputBuffer != null);
            Contract.Requires(outputBuffer != null);
            Contract.Requires(inputBuffer.Length == compUnit.InputInterface.Length);
            Contract.Requires(outputBuffer.Length == compUnit.OutputInterface.Length);
            
            compUnit.InputInterface.WriteValues(inputBuffer, 0);

            for (int iteration = 0; iteration < NumberOfIterations && !cancellationToken.IsCancellationRequested(); iteration++)
            {
                compUnit.ComputeOutput(cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested())
            {
                compUnit.OutputInterface.ReadValues(outputBuffer, 0);
            }
        }
    }
}
