using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public abstract class Body<TBodyPlan> : IBody<TBodyPlan>
        where TBodyPlan : class
    {
        #region Constructors

        protected Body(Guid uid)
        {
            UID = uid;
        }

        protected Body() : this(Guid.NewGuid()) { }

        #endregion

        #region IBody<TBodyPlan> Members

        public abstract TBodyPlan Plan { get; }

        #endregion

        #region IBody Members

        public Guid UID { get; private set; }

        object IBody.Plan
        {
            get { return Plan; }
        }

        #endregion
    }
}
