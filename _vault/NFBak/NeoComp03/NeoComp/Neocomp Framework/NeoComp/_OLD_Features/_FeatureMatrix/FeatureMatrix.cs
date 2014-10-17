using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Features;

namespace NeoComp.Features
{
    public class FeatureMatrix
    {
        internal FeatureMatrix(Matrix<double> matrix, object context = null)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(!matrix.IsEmpty);

            Context = context;
            Matrix = matrix;
            IsNew = true;
        }

        public object Context { get; private set; }
        
        public Matrix<double> Matrix { get; private set; }

        public bool IsNew { get; internal set; }

        public int Height
        {
            get { return Matrix.Height; }
        }
    }
}
