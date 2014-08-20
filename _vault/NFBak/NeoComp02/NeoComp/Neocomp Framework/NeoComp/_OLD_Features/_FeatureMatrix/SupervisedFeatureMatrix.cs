using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    public sealed class SupervisedFeatureMatrix : FeatureMatrix
    {
        internal SupervisedFeatureMatrix(Matrix<double> matrix, Matrix<double> outputMatrix, object context = null)
            : base(matrix, context)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(!matrix.IsEmpty);
            Contract.Requires(outputMatrix != null);
            Contract.Requires(!outputMatrix.IsEmpty);
            Contract.Requires(outputMatrix.Height == matrix.Height);

            OutputMatrix = outputMatrix;
        }
        
        public Matrix<double> OutputMatrix { get; private set; }
    }
}
