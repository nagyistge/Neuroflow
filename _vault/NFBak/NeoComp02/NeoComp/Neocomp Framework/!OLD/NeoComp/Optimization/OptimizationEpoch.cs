using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Features;
using NeoComp.Computations;
using NeoComp.Core;
using NeoComp.Epoch;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization
{
    public class OptimizationEpoch : SynchronizedObject, IEpoch
    {
        #region Constructors

        public OptimizationEpoch(IComputationalUnit<double?> unit, IFeatureMatrixProvider matrixProvider, bool supervised = true)
        {
            Contract.Requires(matrixProvider != null);
            Contract.Requires(unit != null);
            Contract.Requires(!supervised || matrixProvider is ISupervisedFeatureMatrixProvider);
            Contract.Requires((!supervised && unit is IUnsupervisedComputation<double>) || (supervised && unit is ISupervisedComputation<double>));

            Setup(unit, matrixProvider, null, 1, supervised);
        }

        public OptimizationEpoch(IComputationalUnit<double?> unit, IFeatureMatrixProvider matrixProvider, IFeatureMatrixProvider validationMatrixProvider, int validationFrequency = 10, bool supervised = true)
        {
            Contract.Requires(matrixProvider != null);
            Contract.Requires(unit != null);
            Contract.Requires(validationFrequency >= 1);
            Contract.Requires(!supervised || matrixProvider is ISupervisedFeatureMatrixProvider);
            Contract.Requires(!supervised || validationMatrixProvider is ISupervisedFeatureMatrixProvider);
            Contract.Requires((!supervised && unit is IUnsupervisedComputation<double>) || (supervised && unit is ISupervisedComputation<double>));

            Setup(unit, matrixProvider, validationMatrixProvider, validationFrequency, supervised);
        }

        private void Setup(IComputationalUnit<double?> unit, IFeatureMatrixProvider matrixProvider, IFeatureMatrixProvider validationMatrixProvider, int validationFrequency, bool supervised)
        {
            IsSupervised = supervised;
            MatrixProvider = matrixProvider;
            if ((ValidationMatrixProvider = validationMatrixProvider) != null)
            {
                // TODO: Check provider compatibility.
            }
            Unit = unit;
            ValidationFrequency = validationFrequency;
            BestResult = new OptimizationResult(this, true);
            CurrentResult = new OptimizationResult(this);
            BestValidationResult = new OptimizationResult(this, true);
            CurrentValidationResult = new OptimizationResult(this);
        }

        #endregion

        #region Properties And Fields

        int vCounter;

        public bool IsSupervised { get; private set; }

        public IFeatureMatrixProvider MatrixProvider { get; private set; }

        public IFeatureMatrixProvider ValidationMatrixProvider { get; private set; }

        public int ValidationFrequency { get; private set; }

        public IComputationalUnit<double?> Unit { get; private set; }

        OptimizationResult[] results = new OptimizationResult[4];

        public OptimizationResult BestResult
        {
            get { return results[0]; }
            private set { results[0] = value; }
        }

        public OptimizationResult CurrentResult
        {
            get { return results[1]; }
            private set { results[1] = value; }
        }

        public OptimizationResult BestValidationResult
        {
            get { return results[2]; }
            private set { results[2] = value; }
        }

        public OptimizationResult CurrentValidationResult
        {
            get { return results[3]; }
            private set { results[3] = value; }
        }

        bool initialized;

        public bool Initialized
        {
            get { lock (SyncRoot) return initialized; }
        }

        int currentIteration;

        public int CurrentIteration
        {
            get { lock (SyncRoot) return currentIteration; }
        } 

        #endregion

        #region Epoch Logic

        public void Initialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    if (!initialized)
                    {
                        DoInitialization();
                        initialized = true;
                    }
                }
            }
        }

        protected virtual void DoInitialization()
        {
            var reset = Unit as IReset;
            if (reset != null) reset.Reset();
            currentIteration = 0;
            vCounter = 0;
            MatrixProvider.Initialize();
        }

        public void Uninitialize()
        {
            if (initialized)
            {
                lock (SyncRoot)
                {
                    if (initialized)
                    {
                        DoUninitialization();
                        initialized = false;
                    }
                }
            }
        }

        protected virtual void DoUninitialization()
        {
            currentIteration = 0;
            MatrixProvider.Uninitialize();
        }

        public void Step()
        {
            lock (SyncRoot)
            {
                EnsureInitialization();
                OptimizationIteration();
                currentIteration++;
            }
        }

        private void EnsureInitialization()
        {
            if (!initialized)
            {
                DoInitialization();
            }
        }

        #endregion

        #region Iteration

        protected virtual void OptimizationIteration()
        {
            if (IsSupervised)
            {
                SupervisedIteration();
            }
            else
            {
                throw new NotImplementedException("TODO: OptimizationEpoch Unsupervised only."); // TODO: OptimizationEpoch Unsupervised only.
            }
        }

        protected virtual void SupervisedIteration()
        {
            var comp = (ISupervisedComputation<double>)Unit;
            var tprov = (ISupervisedFeatureMatrixProvider)MatrixProvider;
            var vprov = ValidationMatrixProvider != null ? (ISupervisedFeatureMatrixProvider)ValidationMatrixProvider : null;

            var tmatrix = tprov.GetNext();
            var report = tmatrix.Context as IFeatureErrorReport;
            var vmatrix = vprov != null ? vprov.GetNext() : null;
            Matrix<double> tresult, vresult;
            Vector<double> errors = null;

            comp.Compute(tmatrix.Matrix, tmatrix.OutputMatrix, out tresult, true);

            double tmse = 0.0;

            if (report == null)
            {
                tmse = tmatrix.OutputMatrix.ComputeMeanSquareErrorInternal(tresult, comp.Scale);
            }
            else
            {
                int count = tmatrix.OutputMatrix.ItemArray.Length;
                double?[] mseArray = new double?[count];
                for (int idx = 0; idx < count; idx++)
                {
                    var outVec = tmatrix.OutputMatrix.ItemArray[idx];
                    var resultVec = tresult.ItemArray[idx];
                    double e = outVec.ComputeMeanSquareErrorInternal(resultVec, comp.Scale);
                    mseArray[idx] = e;
                    tmse += e;
                }
                tmse /= (double)count;
                errors = Vector.Wrap(mseArray);
            }

            BestResult.Update(tmse, tmatrix, tresult);
            CurrentResult.Update(tmse, tmatrix, tresult);

            if (vmatrix != null)
            {
                if (vCounter++ % ValidationFrequency == 0)
                {
                    comp.Compute(vmatrix.Matrix, vmatrix.OutputMatrix, out vresult, false);
                    double vmse = vmatrix.OutputMatrix.ComputeMeanSquareErrorInternal(vresult, comp.Scale);
                    BestValidationResult.Update(vmse, vmatrix, vresult);
                    CurrentValidationResult.Update(vmse, vmatrix, vresult);
                }
            }

            if (report != null) report.ReportError(errors);
        }

        #endregion
    }
}
