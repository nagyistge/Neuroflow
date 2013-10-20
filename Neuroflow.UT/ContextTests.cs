using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Data;
using System.Diagnostics;
using System.Threading;

namespace Neuroflow.UT
{
    [TestClass]
    public class ContextTests
    {
        #region Device

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public void OCLGetAvailableDevicesTest()
        {
            var devices = OCLContext.GetAvailableDevices();
            Assert.IsNotNull(devices);
            Assert.AreNotEqual(0, devices.Length);
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public void OCLCreateCPUDeviceTest()
        {
            var devices = OCLContext.GetAvailableDevices();

            using (var ctx = new OCLContext("cpu"))
            {
                var device = ctx.Device;
                Assert.IsTrue(devices.Any(d => d.ID == device.ID));
            }

            using (var ctx = new OCLContext("cPu"))
            {
                var device = ctx.Device;
                Assert.IsTrue(devices.Any(d => d.ID == device.ID));
            }

            using (var ctx = new OCLContext("CPU"))
            {
                var device = ctx.Device;
                Assert.IsTrue(devices.Any(d => d.ID == device.ID));
            }
        } 

        #endregion

        #region MSE

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLCalculateMSECPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await CalculateMSETest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLCalculateMSEGPUTest()
        {
            using (var ctx = new OCLContext("Barts"))
            //using (var ctx = new OCLContext("gpu"))
            {
                await CalculateMSETest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        public async Task ManagedCalculateMSETest()
        {
            using (var ctx = new ManagedContext())
            {
                await CalculateMSETest(ctx);
            }
        }

        private async Task CalculateMSETest(ComputationContext ctx)
        {
            const int valuesCount = 1024;
            const int repeat = 10000;

            float[][][] desired = 
            { 
                new[] 
                { 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray(), 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray() 
                }, 
                new[] 
                { 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray(), 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray() 
                } 
            };
            float[][][] current = 
            { 
                new[] 
                { 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray(), 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray() 
                }, 
                new[] 
                { 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray(), 
                    RandomGenerator.NextFloats(-1.0f, 1.0f, valuesCount).ToArray() 
                } 
            };
            float mse = CalcMSE(desired, current);

            using (var batch = new SupervisedBatch())
            using (var resultValues = ctx.DataArrayFactory.Create(2))
            {
                Assert.AreEqual(desired.Length, current.Length);
                for (int i1 = 0; i1 < desired.Length; i1++)
                {
                    float[][] d1 = desired[i1];
                    float[][] c1 = current[i1];
                    var sample = new SupervisedSample();
                    batch.Add(sample);
                    Assert.AreEqual(d1.Length, c1.Length);
                    for (int i2 = 0; i2 < d1.Length; i2++)
                    {
                        float[] d2 = d1[i2];
                        float[] c2 = c1[i2];
                        Assert.AreEqual(d2.Length, c2.Length);
                        var da = ctx.DataArrayFactory.CreateConst(d2);
                        var ca = ctx.DataArrayFactory.CreateConst(c2);
                        sample.Add(da, da, ca);
                    }
                }

                float[] result = new float[2];

                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < repeat; i++)
                {
                    ctx.VectorUtils.CalculateMSE(batch, resultValues, 1);

                    await resultValues.Read(result);

                    Assert.AreEqual(0.0f, result[0]);
                    Assert.AreEqual(Math.Round(mse, 4), Math.Round(result[1], 4));
                }

                sw.Stop();
                Console.WriteLine("Ellapsed: " + sw.ElapsedMilliseconds + " ms");
            }
        }

        private float CalcMSE(float[][][] desired, float[][][] current)
        {
            Assert.AreEqual(desired.Length, current.Length);

            float count = 0.0f;
            float mse = 0.0f;
            Assert.AreEqual(desired.Length, current.Length);
            for (int i1 = 0; i1 < desired.Length; i1++)
            {
                float[][] d1 = desired[i1];
                float[][] c1 = current[i1];
                Assert.AreEqual(d1.Length, c1.Length);
                for (int i2 = 0; i2 < d1.Length; i2++)
                {
                    float[] d2 = d1[i2];
                    float[] c2 = c1[i2];
                    Assert.AreEqual(d2.Length, c2.Length);

                    mse += CalcMSE(d2, c2);
                    count++;
                }
            }

            return mse / count;          
        }

        private float CalcMSE(float[] desired, float[] current)
        {
            float mse = 0.0f;
            for (int i = 0; i < desired.Length; i++)
            {
                float v = (desired[i] - current[i]) * 0.5f;
                mse += v * v;
            }
            return mse / (float)desired.Length;
        }

        #endregion
    }
}
