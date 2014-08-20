using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Core;
using System.Threading;
using System.Diagnostics;

namespace Neuroflow.UnitTests
{
    [TestClass]
    public class DataParallelTests
    {
        [TestMethod]
        public void DoTest()
        {
            PerformDoTest(1);
            int procCount = Environment.ProcessorCount;
            for (int i = 1; i < procCount; i++) PerformDoTest(i);
            PerformDoTest(procCount);
            PerformDoTest(procCount * 100);
            PerformDoTest(procCount * 1000);
        }

        private static void PerformDoTest(int workItemsCount)
        {
            Assert.IsTrue(workItemsCount > 0);
            
            int procCount = Environment.ProcessorCount;
            if (workItemsCount == 1)
            {
                int hitCount = 0;
                DataParallel.Do(workItemsCount, true, (ctx) =>
                {
                    Interlocked.Increment(ref hitCount);
                    Assert.AreEqual(1, ctx.WorkItemsCount);
                    Assert.AreEqual(0, ctx.WorkItemRange.MinValue);
                    Assert.AreEqual(0, ctx.WorkItemRange.MinValue);
                });
                Assert.AreEqual(1, hitCount);
            }
            else if (workItemsCount < procCount)
            {
                int hitCount = 0;
                int[] bits = new int[workItemsCount];
                DataParallel.Do(workItemsCount, true, (ctx) =>
                {
                    Interlocked.Increment(ref hitCount);
                    Assert.AreEqual(workItemsCount, ctx.WorkItemsCount);
                    for (int i = ctx.WorkItemRange.MinValue; i <= ctx.WorkItemRange.MaxValue; i++) bits[i]++;
                });
                Assert.AreEqual(workItemsCount, hitCount);
                foreach (var bit in bits) Assert.AreEqual(1, bit);
            }
            else
            {
                int hitCount = 0;
                int[] bits = new int[workItemsCount];
                DataParallel.Do(workItemsCount, true, (ctx) =>
                {
                    Interlocked.Increment(ref hitCount);
                    Assert.AreEqual(workItemsCount, ctx.WorkItemsCount);
                    for (int i = ctx.WorkItemRange.MinValue; i <= ctx.WorkItemRange.MaxValue; i++) bits[i]++;
                });
                Assert.AreEqual(procCount, hitCount);
                foreach (var bit in bits) Assert.AreEqual(1, bit);
            }
        }
    }
}
