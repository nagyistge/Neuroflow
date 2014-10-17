using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public sealed class BestBodyArrivedEventArgs<TBody> : EventArgs
    {
        public BestBodyArrivedEventArgs(TBody body)
        {
            Contract.Requires(body != null);
                        
            Body = body;
        }

        public TBody Body { get; private set; }
    }
    
    [ContractClass(typeof(ITerritoryContract<,>))]
    public interface ITerritory<TBodyPlan, TBody> : IEnumerable<TBody>, ISelectableItemCollection, ISynchronized
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        bool IsInitialized { get; }
        
        TBody this[int index] { get; }

        void Add(TBody body);

        void Remove(TBody body);

        event EventHandler<BestBodyArrivedEventArgs<TBody>> BestBodyArrived;
    }

    [ContractClassFor(typeof(ITerritory<,>))]
    sealed class ITerritoryContract<TBodyPlan, TBody> : ITerritory<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        bool ITerritory<TBodyPlan, TBody>.IsInitialized
        {
            get { return false; }
        }

        TBody ITerritory<TBodyPlan, TBody>.this[int index]
        {
            get
            {
                ISelectableItemCollection c = this;
                Contract.Requires(index >= 0 && index < c.Count);
                // TODO: Contract.Ensures(Contract.Result<TBody>() != null);
                return null;
            }
        }

        void ITerritory<TBodyPlan, TBody>.Add(TBody body)
        {
            Contract.Requires(body != null);
        }

        void ITerritory<TBodyPlan, TBody>.Remove(TBody body)
        {
            Contract.Requires(body != null);
        }

        event EventHandler<BestBodyArrivedEventArgs<TBody>> ITerritory<TBodyPlan, TBody>.BestBodyArrived
        {
            add { }
            remove { }
        }

        IEnumerator<TBody> IEnumerable<TBody>.GetEnumerator()
        {
            return null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return null;
        }

        int ISelectableItemCollection.Count
        {
            get { return 0; }
        }

        object ISelectableItemCollection.Select(int index)
        {
            return null;
        }

        object ISynchronized.SyncRoot
        {
            get { return null; }
        }
    }
}
