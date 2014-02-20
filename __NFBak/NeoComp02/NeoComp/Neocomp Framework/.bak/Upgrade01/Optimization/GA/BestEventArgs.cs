using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public abstract class BestBodyArrivedEventArgs : EventArgs
    {
        public IBody Body { get; protected set; }
    }

    public sealed class BestBodyArrivedEventArgs<TBodyPlan, TBody> : BestBodyArrivedEventArgs
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        internal BestBodyArrivedEventArgs(TBody body)
        {
            Contract.Requires(body != null);

            Body = body;
        }

        TBody typedBody;

        new public TBody Body
        {
            get { return typedBody; }
            private set
            {
                base.Body = value;
                typedBody = value;
            }
        }
    }

    public abstract class BestBodyArrivedToGroupEventArgs : EventArgs
    {
        public IBody Body { get; protected set; }

        public IGroup Group { get; protected set; }
    }

    public sealed class BestBodyArrivedToGroupEventArgs<TBodyPlan, TBody> : BestBodyArrivedToGroupEventArgs
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        internal BestBodyArrivedToGroupEventArgs(TBody body, Group<TBodyPlan, TBody> group)
        {
            Contract.Requires(body != null);
            Contract.Requires(group != null);

            Body = body;
            Group = group;
        }

        TBody typedBody;

        new public TBody Body
        {
            get { return typedBody; }
            private set
            {
                base.Body = value;
                typedBody = value;
            }
        }

        Group<TBodyPlan, TBody> typedGroup;

        new public Group<TBodyPlan, TBody> Group
        {
            get { return typedGroup; }
            private set
            {
                base.Group = value;
                typedGroup = value;
            }
        }
    }

    public sealed class BestBodyArrivedToPopulationEventArgs : EventArgs
    {
        internal BestBodyArrivedToPopulationEventArgs(IPopulation population, IGroup group, IBody body)
        {
            Contract.Requires(population != null);
            Contract.Requires(group != null);
            Contract.Requires(body != null);

            Population = population;
            Group = group;
            Body = body;
        }
        
        public IBody Body { get; private set; }

        public IGroup Group { get; private set; }

        public IPopulation Population { get; private set; }
    }
}
