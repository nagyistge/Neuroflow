using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Computations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "valIntf")]
    public class ValueInterface<T> : ComputationalInterface<T>, IComputationalInterface<T?>
        where T : struct
    {
        #region Constructor

        public ValueInterface(int length, SyncContext syncRoot)
            : base(length, syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);
        }

        public ValueInterface(ComputationalValue<T>[] values, SyncContext syncRoot)
            : base(values, syncRoot)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length > 0);
            Contract.Requires(syncRoot != null);
        }

        #endregion

        #region T?

        T? IComputationalInterface<T?>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value.HasValue) this[index] = value.Value;
            }
        }

        void IComputationalInterface<T?>.WriteValues(T?[] source, int beginIndex)
        {
            Contract.Assert(!source.IsNullOrEmpty());
            Contract.Assert(beginIndex >= 0 && beginIndex + source.Length <= Length);

            lock (SyncRoot)
            {
                int count = source.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    var sv = source[idx];
                    if (sv.HasValue) Values[idx + beginIndex].Value = sv.Value;
                }
            }
        }

        void IComputationalInterface<T?>.ReadValues(T?[] target, int beginIndex)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Length > 0);
            Contract.Assert(beginIndex >= 0 && beginIndex + target.Length <= Length);

            lock (SyncRoot)
            {
                int targetIndex = 0;
                for (int idx = beginIndex; idx < beginIndex + target.Length && targetIndex < target.Length; idx++, targetIndex++)
                {
                    target[targetIndex] = Values[idx].Value;
                }
            }
        }

        object IComputationalInterface.this[int index]
        {
            get
            {
                return Values[index].Value;
            }
            set
            {
                if (value is T?)
                {
                    var v = (T?)value;
                    if (v.HasValue) Values[index].Value = v.Value;
                }
                else
                {
                    Values[index].Value = value.Cast<T>("value");
                }
            }
        }

        #endregion
    }
}
