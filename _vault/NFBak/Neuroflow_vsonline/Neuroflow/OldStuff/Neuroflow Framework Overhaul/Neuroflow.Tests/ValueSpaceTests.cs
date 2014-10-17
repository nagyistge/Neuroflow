using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Pex.Framework;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Tests
{
    [TestClass, PexClass]
    public partial class ValueSpaceTests
    {
        [PexMethod]
        unsafe public void SetTest(int count, int testValue)
        {
            PexAssume.IsTrue(count > 0 && count < 100);

            using (var space = new ValueSpace<int>(Guid.NewGuid()))
            {
                var indexes = space.Declare(count);

                space.Close();

                foreach (int index in indexes)
                {
                    space[index] = testValue;
                }

                foreach (int index in indexes)
                {
                    PexAssert.IsTrue(space[index] == testValue);
                }
            }
        }
    }
}
