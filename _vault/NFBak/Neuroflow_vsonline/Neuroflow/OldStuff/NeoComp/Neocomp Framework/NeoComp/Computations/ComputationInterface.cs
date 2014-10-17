using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compIntf")]
    public class ComputationInterface<T> : SynchronizedObject, IInputInterface<T>, IOutputInterface<T>
        where T : struct
    {
        public ComputationInterface(int length, SyncContext syncRoot)
            : base(syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);

            values = new ComputationValue<T>[length];
            for (int idx = 0; idx < values.Length; idx++) values[idx] = new ComputationValue<T>();
        }
        
        #region Invariant

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!values.IsNullOrEmpty());
        }

        #endregion

        #region Fields

        [DataMember(Name = "values")]
        ComputationValue<T>[] values;

        #endregion

        #region Properties

        protected internal ComputationValue<T>[] Values
        {
            get { return values; }
        }

        public int Length
        {
            get { return values.Length; }
        }

        #endregion

        #region IInputInterface<T> Members

        public void Write(T?[] inputBuffer)
        {
            lock (SyncRoot)
            {
                int count = inputBuffer.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    var iv = inputBuffer[idx];
                    if (iv.HasValue) values[idx].Value = iv.Value;
                }
            }
        }

        public void Write(T[] inputBuffer)
        {
            lock (SyncRoot)
            {
                int count = inputBuffer.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    values[idx].Value = inputBuffer[idx];
                }
            }
        }

        #endregion

        #region IOutputInterface<T> Members

        public void Read(T?[] outputBuffer)
        {
            lock (SyncRoot)
            {
                int count = outputBuffer.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    outputBuffer[idx] = values[idx].Value;
                }
            }
        }

        public void Read(T[] outputBuffer)
        {
            lock (SyncRoot)
            {
                int count = outputBuffer.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    outputBuffer[idx] = values[idx].Value;
                }
            }
        }

        #endregion

        #region IComputationInterface<T> Members

        public T this[int index]
        {
            get { return values[index].Value; }
            set { values[index].Value = value; }
        }

        #endregion
    }
}
