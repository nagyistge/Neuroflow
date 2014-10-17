using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(IBodyContract))]
    public interface IBody
    {
        Guid UID { get; }

        object Plan { get; }
    }

    [ContractClass(typeof(IBodyContract<>))]
    public interface IBody<TBodyPlan> : IBody
        where TBodyPlan : class
    {
        new TBodyPlan Plan { get; }
    }

    [ContractClassFor(typeof(IBody))]
    sealed class IBodyContract : IBody
    {
        Guid IBody.UID
        {
            get { return default(Guid); }
        }

        object IBody.Plan
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return null;
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            IBody b = this;
            Contract.Invariant(b.UID != default(Guid));
        }
    }

    [ContractClassFor(typeof(IBody<>))]
    sealed class IBodyContract<TBodyPlan> : IBody<TBodyPlan>
        where TBodyPlan : class
    {
        Guid IBody.UID
        {
            get { return default(Guid); }
        }

        object IBody.Plan
        {
            get { return null; }
        }

        TBodyPlan IBody<TBodyPlan>.Plan
        {
            get
            {
                Contract.Ensures(Contract.Result<TBodyPlan>() != null);
                return null;
            }
        }
    }
}
