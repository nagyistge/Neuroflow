using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Pex.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using Neuroflow.Core.ComputationAPI;
using System.Threading.Tasks;
using System.Linq;

namespace Neuroflow.Tests
{
    [TestClass, PexClass]
    public partial class UnsafeParallelTests
    {
        [TestMethod]
        unsafe public void UnsafeParallelIncLegacy()
        {
            UnsafeParallelInc(10);
        }

        [PexMethod]
        unsafe public void UnsafeParallelInc(int count)
        {
            PexAssume.IsTrue(count > 0 && count < 100);

            int size = count * sizeof(int);
            var result = new int[count];

            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Rtl.ZeroMemory(ptr, size);

                // Do It Sequentially

                for (int idx = 0; idx < count; idx++)
                {
                    IncValue(ptr, idx);
                }

                // Do It Parallel

                Parallel.For(0, count,
                    (idx) =>
                    {
                        IncValue(ptr, idx);
                    });

                for (int idx = 0; idx < count; idx++)
                {
                    result[idx] = ((int*)ptr)[idx];
                }

                PexAssert.IsTrue(result.Sum() == count * 2);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        unsafe static void IncValue(IntPtr ptr, int idx)
        {
            ((int*)ptr)[idx]++;
        }
    }
}
