using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal static class DeviceArrayEx
    {
        internal static ManagedArray ToManaged(this IDeviceArray a)
        {
            if (a == null) return null;
            var ma = a as ManagedArray;
            if (ma != null) return ma;
            var da = a as ManagedDataArray;
            if (da != null) return da.InternalManagedArray;
            throw new InvalidCastException("Unable to cast " + a.GetType() + " to ManagedArray.");
        }

        internal static ManagedArray2 ToManaged2(this IDeviceArray a)
        {
            if (a == null) return null;
            var ma = a as ManagedArray2;
            if (ma != null) return ma;
            throw new InvalidCastException("Unable to cast " + a.GetType() + " to ManagedArray2.");
        }
    }
}
