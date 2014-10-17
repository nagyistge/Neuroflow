using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class LearningTestResult : IComputationTestResult
    {
        public LearningTestResult(NeuralNetworkTest test, NeuralNetworkTestResult result)
        {
            Contract.Requires(test != null);
            Contract.Requires(result != null);
            Contract.Requires(test.TestValueUnits.Length == result.Count);

            Test = test;
            Result = result;
        }

        public NeuralNetworkTest Test { get; private set; }

        public NeuralNetworkTestResult Result { get; private set; }

        public double MSE
        {
            get { return Result.MSE; }
        }

        public IEnumerable<IEnumerable<double>> GetErrors()
        {
            return Result.GetErrors();
        }
    }
}
