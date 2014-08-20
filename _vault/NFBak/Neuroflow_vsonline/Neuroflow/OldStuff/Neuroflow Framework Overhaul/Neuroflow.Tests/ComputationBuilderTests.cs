using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Pex.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Core.ComputationAPI;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Neuroflow.Tests
{
    [TestClass, PexClass]
    public partial class ComputationBuilderTests
    {
        [TestMethod, PexMethod(MaxBranches = 40000)]
        public void SimpleComputation()
        {
            using (var space = new ValueSpace<int>(Guid.NewGuid()))
            {
                int v1 = space.Declare();
                int v2 = space.Declare();
                int v3 = space.Declare();

                string v1ref = space.Ref[v1];
                string v2ref = space.Ref[v2];
                string v3ref = space.Ref[v3];

                space.Close();

                var builder = new ComputationBuilder<int>();

                var block1 = new ComputationBlock(0);
                block1.AddReference(typeof(int)); // System
                block1.AddReference(typeof(Assembly)); // System.Reflection

                block1.Add(v1ref + "=5");
                block1.Add(v2ref + "=8");

                var block2 = new ComputationBlock(1);
                block2.AddReference(typeof(StringBuilder)); // System.Text
                block2.AddReference(typeof(int)); // System

                block2.Add(v3ref + "=" + v1ref + "+" + v2ref);

                builder.AddBlock(block1);
                builder.AddBlock(block2);

                var handle = builder.Compile(space, "Test");

                PexAssert.IsNotNull(handle);

                PexAssert.AreEqual(0, space[v1]);
                PexAssert.AreEqual(0, space[v2]);
                PexAssert.AreEqual(0, space[v3]);

                handle.Run();

                PexAssert.AreEqual(5, space[v1], 5);
                PexAssert.AreEqual(8, space[v2], 8);
                PexAssert.AreEqual(5 + 8, space[v3]);
            }
        }

        [TestMethod]
        public void ParallelComputationTest()
        {
            ParallelComputation(1);
        }

        [PexMethod(MaxBranches = 40000)]
        public void ParallelComputation(int parallelLimit)
        {
            PexAssume.InRange(parallelLimit, 1, 17);

            using (var space = new ValueSpace<int>(Guid.NewGuid()))
            {
                int v1 = space.Declare();
                int v2 = space.Declare();
                int v3 = space.Declare();

                string v1ref = space.Ref[v1];
                string v2ref = space.Ref[v2];
                string v3ref = space.Ref[v3];

                space.Close();

                var builder = new ComputationBuilder<int>(parallelLimit);

                var block1 = new ComputationBlock(0);
                block1.Add(v1ref + "=5");

                var block2 = new ComputationBlock(0);
                block2.Add(v2ref + "=8");

                var block3 = new ComputationBlock(1);
                block3.Add(v3ref + "=" + v1ref + "*" + v2ref);

                builder.AddBlock(block3);
                builder.AddBlock(block1);
                builder.AddBlock(block2);                

                var handle = builder.Compile(space, "Test");

                PexAssert.IsNotNull(handle);

                PexAssert.AreEqual(0, space[v1]);
                PexAssert.AreEqual(0, space[v2]);
                PexAssert.AreEqual(0, space[v3]);

                handle.Run();

                PexAssert.AreEqual(5, space[v1], 5);
                PexAssert.AreEqual(8, space[v2], 8);
                PexAssert.AreEqual(5 * 8, space[v3]);
            }
        }
    }
}
