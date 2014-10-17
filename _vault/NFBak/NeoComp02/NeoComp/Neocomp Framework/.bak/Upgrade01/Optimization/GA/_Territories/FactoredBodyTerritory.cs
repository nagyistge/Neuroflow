using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.GA
{
    public class FactoredBodyTerritory<TBodyPlan, TBody> : ComparableBodyTerritoryBase<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        #region Observer

        class FactoredBodyObserver : BodyObserver
        {
            public ISuccessFactor SuccessFactor { get; internal set; }
        }

        #endregion

        #region Comparer

        class BodyComparer : BodyComparerBase
        {
            public override int Compare(ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyObserver x, ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyObserver y)
            {
                var xo = (FactoredBodyObserver)x;
                var yo = (FactoredBodyObserver)y;
                int c = xo.SuccessFactor.CompareTo(yo.SuccessFactor);
                if (c != 0) return c;
                return base.Compare(x, y);
            }
        }

        #endregion

        #region Constructor

        public FactoredBodyTerritory() : base() { }

        #endregion

        #region Create Comparer

        protected override ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyComparerBase CreateComparer()
        {
            return new BodyComparer();
        }

        #endregion

        #region Create Observer

        protected override ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyObserver CreateObserver(TBody forBody)
        {
            var obs = new FactoredBodyObserver();
            try
            {
                obs.SuccessFactor = SuccessFactorFactory.CreateFromObject(forBody);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Cannot create success factor for body '" + forBody + "'. See inner exception for details.", ex);
            }
            return obs;
        }

        #endregion
    }
}
