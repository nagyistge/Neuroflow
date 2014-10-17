using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public abstract class PlannedBody<TBodyPlan> : Body<TBodyPlan>
        where TBodyPlan : class
    {
        #region Constructors

        protected PlannedBody(TBodyPlan plan)
            : base()
        {
            Contract.Requires(plan != null);
            
            this.plan = plan;
        }

        protected PlannedBody(Guid uid, TBodyPlan plan)
            : base(uid)
        {
            Contract.Requires(plan != null);

            this.plan = plan;
        } 

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(plan != null);
        }

        #endregion

        #region Properties

        TBodyPlan plan;

        public override TBodyPlan Plan
        {
            get { return plan; }
        } 

        #endregion
    }
}
