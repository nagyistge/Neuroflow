using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Globalization;

namespace NeoComp.Computations
{
    public abstract class ComputationTestResult<TI, TO> : ReadOnlyCollection<ComputationTestResultEntry<TI, TO>>, IComputationTestResult
    {
        protected ComputationTestResult(Guid testUID, IList<ComputationTestResultEntry<TI, TO>> entryList)
            : base(entryList)
        {
            Contract.Requires(!entryList.IsNullOrEmpty());

            TestUID = testUID;
        }

        object mseLock = new object();

        public Guid TestUID { get; private set; }

        double? mse;

        public double MSE
        {
            get
            {
                if (!mse.HasValue)
                {
                    lock (mseLock)
                    {
                        if (!mse.HasValue)
                        {
                            mse = GetErrors().MeanSquare();
                        }
                    }
                }
                return mse.Value;
            }
        }

        public IEnumerable<IEnumerable<double>> GetErrors()
        {
            foreach (var entry in this)
            {
                yield return GetErrors(entry);
            }
        }

        private IEnumerable<double> GetErrors(ComputationTestResultEntry<TI, TO> entry)
        {
            Contract.Assert(entry.ComputedValues.Length == entry.TestUnit.OutputValues.Length);

            int length = entry.ComputedValues.Length;
            for (int idx = 0; idx < length; idx++)
            {
                TI computedValue = entry.ComputedValues[idx];
                TO desiredValue = entry.TestUnit.OutputValues[idx];
                yield return GetError(computedValue, desiredValue);
            }
        }

        protected abstract double GetError(TI computedValue, TO desiredValue);

        public string ToString(bool withMSE)
        {
            var sb = new StringBuilder();
            foreach (var entry in this)
            {
                if (sb.Length != 0) sb.AppendLine();
                sb.Append(entry.ToString());
            }
            if (withMSE)
            {
                sb.AppendLine();
                sb.Append("MSE: ");
                sb.Append(MSE);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(true);
        }
    }
}
