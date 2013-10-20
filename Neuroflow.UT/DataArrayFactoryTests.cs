using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuroflow.UT
{
    [TestClass]
    public class DataArrayFactoryTests
    {
        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        public async Task ManagedCreateDataArrayByFillTest()
        {
            using (var ctx = new ManagedContext())
            {
                await CreateDataArrayByFillTest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLCreateDataArrayByFillCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await CreateDataArrayByFillTest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLCreateDataArrayByFillGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await CreateDataArrayByFillTest(ctx);
            }
        }

        private static async Task CreateDataArrayByFillTest(ComputationContext ctx)
        {
            int count = 10;
            using (var array = ctx.DataArrayFactory.Create(count, 1.1f))
            {
                var sw = new Stopwatch();
                sw.Start();
                var values = new float[count];
                await array.Read(values);

                foreach (var v in values) Assert.AreEqual(1.1f, v);

                var valuesToWrite = new float[count];
                for (int i = 0; i < valuesToWrite.Length; i++) valuesToWrite[i] = 5.5f;

                await array.Write(valuesToWrite);

                await array.Read(values);

                foreach (var v in values) Assert.AreEqual(5.5f, v);
                sw.Stop();

                Console.WriteLine("CreateDataArrayByFillTest: " + sw.ElapsedMilliseconds + " ms");
            }
        }
    }
}
