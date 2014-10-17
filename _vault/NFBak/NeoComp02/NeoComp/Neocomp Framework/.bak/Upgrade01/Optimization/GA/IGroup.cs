using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(IGroupContract))]
    public interface IGroup : ISynchronized
    {
        int Size { get; set; }

        IntRange ParentCount { get; set; }

        ISelectionStrategy SelectionStrategy { get; set; }

        event EventHandler<BestBodyArrivedEventArgs> BestBodyArrived;
    }

    [ContractClassFor(typeof(IGroup))]
    public class IGroupContract : IGroup
    {
        int IGroup.Size
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
            set
            {
                Contract.Requires(value > 0);
            }
        }

        IntRange IGroup.ParentCount
        {
            get
            {
                Contract.Ensures(!Contract.Result<IntRange>().IsZero);
                return default(IntRange);
            }
            set
            {
                Contract.Requires(!value.IsZero);
            }
        }

        ISelectionStrategy IGroup.SelectionStrategy
        {
            get
            {
                Contract.Ensures(Contract.Result<ISelectionStrategy>() != null);
                return null;
            }
            set
            {
                Contract.Requires(value != null);
            }
        }

        event EventHandler<BestBodyArrivedEventArgs> IGroup.BestBodyArrived
        {
            add { }
            remove { }
        }

        object ISynchronized.SyncRoot
        {
            get { return null; }
        }
    }
}
