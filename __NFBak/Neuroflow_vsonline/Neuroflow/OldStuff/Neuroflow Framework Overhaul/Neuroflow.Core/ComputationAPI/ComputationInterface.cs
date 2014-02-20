using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Core.ComputationAPI
{
    public static class ComputationInterface
    {
        #region Double

        unsafe public static double FastGet(this ComputationInterface<double> intf, int index)
        {
            return intf.ValueSpace.FastGet(intf.ReferenceIndexes[index]);
        }

        unsafe public static void FastSet(this ComputationInterface<double> intf, int index, double value)
        {
            intf.ValueSpace.FastSet(intf.ReferenceIndexes[index], value);
        }

        unsafe public static void FastWrite(this ComputationInterface<double> intf, double?[] inputVector)
        {
            int len = intf.Length;
            for (int i = 0; i < len; i++)
            {
                double? inputValue = inputVector[i];
                if (inputValue != null) intf.ValueSpace.FastSet(intf.ReferenceIndexes[i], inputValue.Value);
            }
        }

        unsafe public static void Zero(this ComputationInterface<double> intf)
        {
            int len = intf.Length;
            for (int i = 0; i < len; i++)
            {
                intf.ValueSpace.FastSet(intf.ReferenceIndexes[i], 0);
            }
        }

        #endregion

        #region Bool

        unsafe public static bool FastGet(this ComputationInterface<bool> intf, int index)
        {
            return intf.ValueSpace.FastGet(intf.ReferenceIndexes[index]);
        }

        unsafe public static void FastSet(this ComputationInterface<bool> intf, int index, bool value)
        {
            intf.ValueSpace.FastSet(intf.ReferenceIndexes[index], value);
        }

        unsafe public static void FastWrite(this ComputationInterface<bool> intf, bool?[] inputVector)
        {
            int len = intf.Length;
            for (int i = 0; i < len; i++)
            {
                bool? inputValue = inputVector[i];
                if (inputValue != null) intf.ValueSpace.FastSet(intf.ReferenceIndexes[i], inputValue.Value);
            }
        }

        #endregion
    }

    public class ComputationInterface<T> : SynchronizedObject where T : struct
    {
        public ComputationInterface(SyncContext syncRoot, ValueSpace<T> valueSpace, int length)
            : base(syncRoot)
        {
            Contract.Requires(syncRoot != null);
            Contract.Requires(valueSpace != null);
            Contract.Requires(length >= 0);

            this.ValueSpace = valueSpace;

            if (length != 0)
            {
                ReferenceIndexes = valueSpace.Declare(length);
            }
            else
            {
                ReferenceIndexes = new int[0];
            }
        }

        public ComputationInterface(SyncContext syncRoot, ValueSpace<T> valueSpace, IEnumerable<int> referenceIndexCollection)
            : base(syncRoot)
        {
            Contract.Requires(syncRoot != null);
            Contract.Requires(valueSpace != null);
            Contract.Requires(referenceIndexCollection != null);

            this.ValueSpace = valueSpace;
            ReferenceIndexes = referenceIndexCollection.ToArray();
        }

        internal ValueSpace<T> ValueSpace { get; private set; }

        internal int[] ReferenceIndexes { get; private set; }

        public int Length
        {
            get { return ReferenceIndexes.Length; }
        }

        public T this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < Length);
                lock (SyncRoot) return ValueSpace[ReferenceIndexes[index]];
            }
            set
            {
                Contract.Requires(index >= 0 && index < Length);
                lock (SyncRoot) ValueSpace[ReferenceIndexes[index]] = value;
            }
        }

        public int GetReferenceIndex(int index)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(index < Length);

            return ReferenceIndexes[index];
        }

        public void Write(T?[] inputVector)
        {
            Contract.Requires(inputVector != null);
            Contract.Requires(inputVector.Length == Length);

            for (int i = 0; i < Length; i++)
            {
                T? inputValue = inputVector[i];
                if (inputValue != null) ValueSpace[ReferenceIndexes[i]] = inputValue.Value;
            }
        }
    }
}
