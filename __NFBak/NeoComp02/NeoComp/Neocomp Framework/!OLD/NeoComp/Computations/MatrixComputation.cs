using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public delegate void ActionAfterRowVectorComputation<T>(Vector<T> rowVector, int rowVectorIndex) where T : struct;
    
    public class MatrixComputation<T> : Learningutation<T>
        where T : struct
    {
        public MatrixComputation(int numberOfIterations = 1)
            : base(numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
        }

        public Matrix<T> Compute(IComputationalUnit<T?> compUnit, Matrix<T> inputMatrix, ActionAfterRowVectorComputation<T> actionAfterRowVectorComputation = null, CancellationToken? cancellationToken = null)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputMatrix != null);
            Contract.Requires(inputMatrix.Width > 0 && inputMatrix.Height > 0);

            var sync = compUnit as ISynchronized;
            bool lockTaken = false;
            try
            {
                if (sync != null) Monitor.Enter(sync.SyncRoot, ref lockTaken);

                var matrixBuffer = new Vector<T>[inputMatrix.Count];
                for (int idx = 0; idx < inputMatrix.Count; idx++)
                {
                    var inputVector = inputMatrix[idx];
                    var vectorBuffer = new T?[compUnit.OutputInterface.Length];
                    SafeComputeInternal(compUnit, inputVector.ItemArray, vectorBuffer, cancellationToken);
                    var rowVector = Vector.Wrap(vectorBuffer);
                    if (actionAfterRowVectorComputation != null)
                    {
                        actionAfterRowVectorComputation(rowVector, idx);
                    }
                    matrixBuffer[idx] = rowVector;
                }
                return Matrix.Wrap(matrixBuffer);
            }
            finally
            {
                if (sync != null && lockTaken) Monitor.Exit(sync.SyncRoot);
            }
        }
    }
}
