using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Adjustables
{
    [ContractClass(typeof(IRangedAdjustableItemContract))]
    public interface IRangedAdjustableItem : IAdjustableItem
    {
        DoubleRange Range { get; }
    }

    [ContractClassFor(typeof(IRangedAdjustableItem))]
    class IRangedAdjustableItemContract : IRangedAdjustableItem
    {
        DoubleRange IRangedAdjustableItem.Range
        {
            get { return default(DoubleRange); }
        }

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            IRangedAdjustableItem la = this;
            Contract.Invariant(!la.Range.IsFixed);
            Contract.Invariant(la.Range.IsIn(la.Adjustment));
        }

        double IAdjustableItem.Adjustment
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
