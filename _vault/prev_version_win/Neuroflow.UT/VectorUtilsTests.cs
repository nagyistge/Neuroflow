using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.UT
{
    [TestClass]
    public class VectorUtilsTests
    {
        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory("CPU")]
        public async Task OCLZeroBufferCPUTest()
        {
            using (var ctx = new OCLContext("CPU"))
            {
                await DoTest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory("GPU")]
        public async Task OCLZeroBufferGPUTest()
        {
            using (var ctx = new OCLContext("GPU"))
            {
                await DoTest(ctx);
            }
        }

        private async Task DoTest(ComputationContext ctx)
        {
            await DoTest(ctx, 4096, 4911);
        }

        private async Task DoTest(ComputationContext ctx, int fromSize, int toSize)
        {
            for (int size = fromSize; size <= toSize; size++) await DoTest(ctx, size);
        }

        private async Task DoTest(ComputationContext ctx, int size)
        {
            float[] notZeros = Enumerable.Repeat(1.1f, size).ToArray();
            float[] values = new float[size];
            using (var data = ctx.DataArrayFactory.Create(notZeros))
            {
                await data.Read(values);

                Assert.IsTrue(values.All(v => v == 1.1f));

                ctx.VectorUtils.Zero(data);

                await data.Read(values);

                Assert.IsTrue(values.All(v => v == 0.0f));
            }
        }
    }
}
