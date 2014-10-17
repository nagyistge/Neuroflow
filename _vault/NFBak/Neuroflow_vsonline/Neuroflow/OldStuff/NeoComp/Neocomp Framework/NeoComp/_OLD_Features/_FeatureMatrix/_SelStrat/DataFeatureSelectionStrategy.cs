using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(DataFeatureSelectionStrategyContract))]
    public abstract class DataFeatureSelectionStrategy
    {
        #region Fields and Properties

        protected DataFeatureMatrixProvider Owner { get; private set; }

        #endregion

        #region Init

        internal void Initialize(DataFeatureMatrixProvider owner)
        {
            Contract.Requires(owner != null);
            
            if (Owner != null && Owner != owner)
            {
                throw new InvalidOperationException(this + " is already in use by " + Owner + ".");
            }

            Owner = owner;

            Initialize();
        }

        protected abstract void Initialize();

        protected internal abstract void Uninitialize();

        #endregion

        #region Get Next Indexes

        /// <summary>
        /// Get next indexes in a set.
        /// </summary>
        /// <returns>Set - next, null = same as previous</returns>
        protected internal abstract FeatureIndexSet? GetNextIndexes();

        #endregion
    }

    [ContractClassFor(typeof(DataFeatureSelectionStrategy))]
    abstract class DataFeatureSelectionStrategyContract : DataFeatureSelectionStrategy
    {
    }
}
