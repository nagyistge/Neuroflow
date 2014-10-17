using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.OpenCL;
using Cloo;

namespace Neuroflow.UnitTests
{
    [TestClass]
    public class OpenCLFeatureTests
    {
        [TestMethod]
        public void SubBufferTest()
        {
            using (var ctx = new OpenCLContext())
            {
                var cpuDev = ctx.Devices.Where(d => d.Type == ComputeDeviceTypes.Cpu).Single();
                var gpuDev = ctx.Devices.Where(d => d.Type == ComputeDeviceTypes.Gpu).Single();

                DoSubBufferTest(ctx, cpuDev);
                DoSubBufferTest(ctx, gpuDev);
            }
        }

        private static void DoSubBufferTest(OpenCLContext context, ComputeDevice device)
        {
            int size = 10;
            int size2 = size / 2;

            using (var buff = context.CreateBuffer<float>(size, ComputeMemoryFlags.ReadWrite))
            //using (var subBuff = context.CreateSubBuffer<double>(buff, ComputeMemoryFlags.ReadWrite, 1, size2))
            using (var q = context.CreateQueue(device))
            {
                var values = new float[size];
                var firstValues = new float[size2];

                Fill(values, 3.4f);
                IsFilledWith(values, 3.4f);

                q.WriteAll(buff, values);

                Fill(values, 0f);
                IsFilledWith(values, 0f);

                q.ReadAll(buff, values);
                IsFilledWith(values, 3.4f);

                IsFilledWith(firstValues, 0f);

                //q.ReadAll(subBuff, firstValues);
                //IsFilledWith(firstValues, 3.4f);

                //q.ComputeCommandQueue.ReadFromBuffer(buff.ComputeBufferBase, ref firstValues, true, 1, 0, size2, null);

                q.Read(buff, firstValues, 2, size2);

                IsFilledWith(firstValues, 3.4f);

                Fill(firstValues, 5.0f);
                q.Write(buff, firstValues, 2, size2);

                q.ReadAll(buff, values);
            }
        }

        private static void Fill<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++) array[i] = value;
        }

        private static void IsFilledWith<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(value, array[i]);
            }
        }
    }
}
