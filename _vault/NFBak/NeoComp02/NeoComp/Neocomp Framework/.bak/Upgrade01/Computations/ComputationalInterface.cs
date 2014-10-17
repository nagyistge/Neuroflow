using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections;
using NeoComp.Computations;

namespace NeoComp.Core
{
    public class ComputationalInterface<T> : SynchronizedObject, IComputationalInterface<T>
    {
        #region Constructor

        public ComputationalInterface(int length, object syncRoot)
            : base(syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);

            values = new ComputationalValue<T>[length];
            for (int idx = 0; idx < values.Length; idx++) values[idx] = new ComputationalValue<T>();
        }

        public ComputationalInterface(ComputationalValue<T>[] values, object syncRoot)
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

        public void WriteValues(IList<T> source, int beginIndex = 0)
        {
            Contract.Assert(!source.IsNullOrEmpty());
            Contract.Assert(beginIndex >= 0 && beginIndex + source.Count <= Length);

            lock (SyncRoot)
            {
                int count = source.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    values[idx + beginIndex].Value = source[idx];
                }
            }
        }

        public void ReadValues(IList<T> target, int count, int beginIndex = 0) // TODO: Preinitialized size list!!!!
        {
            Contract.Assert(target != null);
            Contract.Assert(count > 0);
            Contract.Assert(beginIndex >= 0 && beginIndex + count <= Length);

            lock (SyncRoot)
            {
                for (int idx = beginIndex; idx < beginIndex + count; idx++)
                {
                    target.Add(values[idx].Value); 
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
