using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.GA
{
    public class ComparableBodyTerritory<TBodyPlan, TBody> : ComparableBodyTerritoryBase<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>, IComparable<TBody>
    {
        #region Comparer

        class BodyComparer : BodyComparerBase
        {
            public override int Compare(ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyObserver x, ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyObserver y)
            {
                int c = x.Body.CompareTo(y.Body);
                if (c != 0) return c;
                return base.Compare(x, y);
            }
        }

        #endregion

        #region Constructor

        public ComparableBodyTerritory() : base() { }

        #endregion

        #region Get Comparer

        protected override ComparableBodyTerritoryBase<TBodyPlan, TBody>.BodyComparerBase CreateComparer()
        {
            return new BodyComparer();
        }

        #endregion
    }
}
