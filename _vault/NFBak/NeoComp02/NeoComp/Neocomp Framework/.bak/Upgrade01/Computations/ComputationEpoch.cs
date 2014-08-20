using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using NeoComp.Core;
using System.Collections;

namespace NeoComp.Computations
{
    public class ComputationEpoch<T>
    {
        #region Constructors

        public ComputationEpoch(int outputSize,
            int inputBeginIndex = 0,
            int outputBeginIndex = 0,
            int numberOfIterations = 1,
            CancellationToken? cancellationToken = null)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(inputBeginIndex >= 0);
            Contract.Requires(outputBeginIndex >= 0);
            Contract.Requires(outputSize > 0);

            NumberOfIterations = numberOfIterations;
            InputBeginIndex = inputBeginIndex;
            OutputBeginIndex = outputBeginIndex;
            OutputSize = outputSize;
            this.cancellationToken = cancellationToken;
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(NumberOfIterations > 0);
            Contract.Invariant(InputBeginIndex >= 0);
            Contract.Invariant(OutputBeginIndex >= 0);
            Contract.Invariant(OutputSize > 0);
        }

        #endregion

        #region Fields

        CancellationToken? cancellationToken;

        #endregion

        #region Properties

        public int NumberOfIterations { get; private set; }

        public int InputBeginIndex { get; private set; }

        public int OutputBeginIndex { get; private set; }

        public int OutputSize { get; private set; }

        bool IsCancellationRequested
        {
            get { return cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested; }
        }

        #endregion

        #region Run

        public void Run(IComputationalUnit<T> computationalUnit, IList<T> inputValues, IList<T> result)
        {
            Contract.Requires(computationalUnit != null);
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(result != null);
            Contract.Requires(InputBeginIndex + inputValues.Count <= computationalUnit.InputInterface.Length);
            Contract.Requires(OutputBeginIndex + OutputSize <= computationalUnit.OutputInterface.Length);

            var sync = computationalUnit as ISynchronized; 
            if (sync != null) Monitor.Enter(sync.SyncRoot);
            try
            {
                computationalUnit.InputInterface.WriteValues(inputValues, InputBeginIndex);

                for (int iteration = 0; iteration < NumberOfIterations && !IsCancellationRequested; iteration++)
                {
                    computationalUnit.ComputeOutput(cancellationToken);
                }

                if (!IsCancellationRequested)
                {
                    computationalUnit.OutputInterface.ReadValues(result, OutputSize, OutputBeginIndex); 
                }
            }
            finally
            {
                if (sync != null) Monitor.Exit(sync.SyncRoot);
            }
        }

        #endregion
    }
}
