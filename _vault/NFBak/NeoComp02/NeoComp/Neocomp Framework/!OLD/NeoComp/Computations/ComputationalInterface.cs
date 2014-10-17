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
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compIntf")]
    public class ComputationalInterface<T> : SynchronizedObject, IComputationalInterface<T>
    {
        #region Constructor

        public ComputationalInterface(int length, SyncContext syncRoot)
            : base(syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);

            values = new ComputationalValue<T>[length];
            for (int idx = 0; idx < values.Length; idx++) values[idx] = new ComputationalValue<T>();
        }

        public ComputationalInterface(ComputationalValue<T>[] values, SyncContext syncRoot)
            : base(syncRoot)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length > 0);
            Contract.Requires(syncRoot != null);

            this.values = values;
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(!values.IsNullOrEmpty());
        }

        #endregion

        #region Fields

        [DataMember(Name = "values")]
        ComputationalValue<T>[] values;

        #endregion

        #region Properties

        protected ComputationalValue<T>[] Values
        {
            get { return values; }
        }

        public int Length
        {
            get { return values.Length; }
        }

        #endregion

        #region Read / Write

        public void WriteValues(T[] source, int beginIndex = 0)
        {
            Contract.Assert(!source.IsNullOrEmpty());
            Contract.Assert(beginIndex >= 0 && beginIndex + source.Length <= Length);

            lock (SyncRoot)
            {
                int count = source.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    values[idx + beginIndex].Value = source[idx];
                }
            }
        }

        public void ReadValues(T[] target, int beginIndex = 0)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Length > 0);
            Contract.Assert(beginIndex >= 0 && beginIndex + target.Length <= Length);

            lock (SyncRoot)
            {
                int targetIndex = 0;
                for (int idx = beginIndex; idx < beginIndex + target.Length && targetIndex < target.Length; idx++, targetIndex++)
                {
                    target[targetIndex] = values[idx].Value;
                }
            }
        }

        #endregion

        #region Impl

        public T this[int index]
        {
            get
            {
                return values[index].Value;
            }
            set
            {
                values[index].Value = value;
            }
        }

        object IComputationalInterface.this[int index]
        {
            get
            {
                return values[index].Value;
            }
            set
            {
                values[index].Value = value.Cast<T>("value");
            }
        }

        #endregion
    }
}
