using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using ColorTest;
using Neuroflow.Core.Algorithms.Selection;
using System.Diagnostics;

namespace ColorTestUT
{
    [TestClass]
    public class GATests
    {
        [TestMethod]
        public void GATest()
        {
            // Target:
            double num = 0.55;

            // Algo
            Func<Genom, double> f = g => Math.Abs(num - g.Genes.Sum());
            var algo = new TournamentSelectionAlgorithm(5);
            var ga = new GA(10, 100, 1, new PointMutationPars(PointMutationType.Uniform, 0.05, 0.1), algo, new FitnessFunctionComparer(f));
            ga.Initialize();

            // Run
            int maxGen = 100;
            for (int i = 0; i < maxGen; i++)
            {
                ga.NextGeneration();
                Debug.WriteLine(f(ga[0]) + " " + f(ga[1]));
            }

            Debug.WriteLine(ga[0]);
        }
    }
}
