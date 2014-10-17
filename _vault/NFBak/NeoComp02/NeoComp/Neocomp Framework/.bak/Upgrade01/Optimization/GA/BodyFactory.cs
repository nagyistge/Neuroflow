using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(BodyFactoryContract<,>))]
    public abstract class BodyFactory<TBodyPlan, TBody> : SynchronizedObject
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        #region Recombination

        internal TBody CreateOffspring(IEnumerable<TBody> parents, CancellationToken? cancellationToken)
        {
            Contract.Requires(parents != null);
                        
            TBodyPlan first = null, current = null;
            foreach (var parent in parents)
            {
                if (parent != null)
                {
                    var plan = parent.Plan;
                    Contract.Assert(plan != null);

                    if (cancellationToken.IsCancellationRequested()) return null;
                    if (current == null)
                    {
                        first = current = plan;
                    }
                    else
                    {
                        current = Crossover(current, plan);
                    }
                }
            }

            if (current == null || cancellationToken.IsCancellationRequested())
            {
                return null;
            }

            current = Mutate(current, first == current);

            if (current == first) throw new InvalidOperationException(ToString() + " has not built a plan copy.");

            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return null;

            return CreateAndInitializeBody(current, cancellationToken);
        }

        protected abstract TBodyPlan Crossover(TBodyPlan plan1, TBodyPlan plan2);

        protected virtual TBodyPlan Mutate(TBodyPlan plan, bool copyNeeded)
        {
            Contract.Requires(plan != null);
            Contract.Ensures(Contract.Result<TBodyPlan>() != null); 
            
            return plan;
        }

        #endregion

        #region Creation

        internal IEnumerable<TBody> CreateRandomBodies(int count, CancellationToken? cancellationToken)
        {
            Contract.Requires(count > 0);

            for (int idx = 0; idx < count && !cancellationToken.IsCancellationRequested(); idx++)
            {
                var plan = CreateInitialPlan();
                var body = CreateAndInitializeBody(plan, cancellationToken);
                if (body != null)
                {
                    yield return body;
                }
                else
                {
                    idx--;
                }
            }
        }

        private TBody CreateAndInitializeBody(TBodyPlan plan, CancellationToken? cancellationToken)
        {
            var body = CreateBody(plan);
            if (cancellationToken.IsCancellationRequested()) return null;
            if (body != null) Initialize(body, cancellationToken);
            return body;
        }

        private void Initialize(TBody body, CancellationToken? cancellationToken)
        {
            var i = body as IInitializable;
            if (i != null)
            {
                try
                {
                    i.Initialize(cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(body.ToString() + " initialization failed. See inner exception for details.", ex);
                }
            }
        }

        protected abstract TBody CreateBody(TBodyPlan plan);

        protected abstract TBodyPlan CreateInitialPlan();

        #endregion
    }

    [ContractClassFor(typeof(BodyFactory<,>))]
    internal abstract class BodyFactoryContract<TBodyPlan, TBody> : BodyFactory<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        protected override TBodyPlan Crossover(TBodyPlan plan1, TBodyPlan plan2)
        {
            Contract.Requires(plan1 != null);
            Contract.Requires(plan2 != null);
            Contract.Ensures(Contract.Result<TBodyPlan>() != null);
            
            return null;
        }

        protected override TBody CreateBody(TBodyPlan plan)
        {
            Contract.Requires(plan != null);
            Contract.Ensures(Contract.Result<TBody>() != null); 
            
            return null;
        }

        protected override TBodyPlan CreateInitialPlan()
        {
            Contract.Ensures(Contract.Result<TBodyPlan>() != null); 
            
            return null;
        }
    }
}
