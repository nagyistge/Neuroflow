using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using NeoComp.Core;

namespace NeoComp.Optimization.GA
{
    public sealed class ObjectSuccessFactor<TContainer> : SuccessFactor<TContainer>
        where TContainer : class
    {
        #region Constructor

        public ObjectSuccessFactor(TContainer container)
            : base(container, ComparationMode.None)
        {
            Update();
        } 

        #endregion

        #region Properties

        public SuccessFactor<TContainer>[] LastKnownFactors { get; private set; } 

        #endregion

        #region Update

        public override void Update()
        {
            var sync = Container as ISynchronized;
            if (sync != null) Monitor.Enter(sync.SyncRoot);
            try
            {
                Build();
            }
            finally
            {
                if (sync != null) Monitor.Exit(sync.SyncRoot);
            }
        } 

        #endregion

        #region Build

        private void Build()
        {
            var last = LastKnownFactors;
            LastKnownFactors = SuccessFactorFactory.GetFactors(Container).ToArray();
            RemoveFactorHandlers(last);
            AddFactorHandlers();
        }

        private void AddFactorHandlers()
        {
            foreach (var factor in LastKnownFactors)
            {
                factor.Dirtied += new EventHandler(OnFactorDirtied);
            }
        }

        private void RemoveFactorHandlers(ISuccessFactor[] last)
        {
            if (!last.IsNullOrEmpty())
            {
                foreach (var factor in last)
                {
                    factor.Dirtied += new EventHandler(OnFactorDirtied);
                }
            }
        }

        #endregion

        #region Factor Callback

        void OnFactorDirtied(object sender, EventArgs e)
        {
            ToDirty();
        } 

        #endregion

        #region Compare

        protected override int DoCompareTo(SuccessFactor<TContainer> sfOther)
        {
            var other = (ObjectSuccessFactor<TContainer>)sfOther;
            var factors = LastKnownFactors;
            var otherFactors = other.LastKnownFactors;
            Debug.Assert(factors != null);
            Debug.Assert(otherFactors != null);
            int count = factors.Length;
            Debug.Assert(count == otherFactors.Length);

            int myValue = 0, otherValue = 0;

            for (int idx = 0; idx < count; idx++)
            {
                int comp = factors[idx].CompareTo(otherFactors[idx]);
                if (comp != 0) return comp;
            }

            return myValue.CompareTo(otherValue);
        }

        #endregion
    }
}
