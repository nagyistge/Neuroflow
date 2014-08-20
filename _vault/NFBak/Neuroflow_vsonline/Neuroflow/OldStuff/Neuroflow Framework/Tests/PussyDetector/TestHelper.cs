using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using Neuroflow.Core;
using System.Threading.Tasks;

namespace PussyDetector
{
    public struct TestResult
    {
        public TestResult(double successPercent, int falseNegativeCount, int falsePositiveCount)
        {
            this.successPercent = successPercent;
            this.falseNegativeCount = falseNegativeCount;
            this.falsePositiveCount = falsePositiveCount;
        }
        
        double successPercent;

        public double SuccessPercent
        {
            get { return successPercent; }
        }

        int falseNegativeCount;

        public int FalseNegativeCount
        {
            get { return falseNegativeCount; }
        }

        int falsePositiveCount;

        public int FalsePositiveCount
        {
            get { return falsePositiveCount; }
        }

        public override string ToString()
        {
            return string.Format("Success: {0}%, False Negatives: {1}, False Positives: {2}", SuccessPercent, FalseNegativeCount, FalsePositiveCount);
        }
    }
    
    public static class TestHelper
    {
        static List<NeuralVectors> validationVectors;
        
        public static void TestNetwork(NeuralNetwork network, IUnorderedNeuralVectorsProvider validationProvider)
        {
            Contract.Requires(network != null);

            var prov = (FullSampleProvider)validationProvider;

            if (validationVectors == null) validationVectors = prov.GetAllVectors().ToList();

            var clone = network.Clone();

            Task.Factory.StartNew(() =>
            {
                var reset = clone.GetReset();
                int fnCount = 0;
                int fpCount = 0;
                int okCount = 0;
                int count = 0;

                foreach (var vectors in validationVectors)
                {
                    bool isDetectable = false;

                    foreach (var vector in vectors)
                    {
                        for (int idx = 0; idx < vector.InputVector.Length; idx++)
                        {
                            clone.InputInterface[idx] = vector.InputVector[idx].Value;
                        }

                        for (int i = 0; i < vector.NumberOfIterations; i++)
                        {
                            clone.Iteration();
                        }

                        if (vector.DesiredOutputVector[0] != null)
                        {
                            isDetectable = vector.DesiredOutputVector[0].Value > 0.0;
                        }
                    }

                    double detectedOutput = clone.OutputInterface[0];

                    bool isDetectedByNetwork = detectedOutput > 0.0;

                    if (isDetectedByNetwork == isDetectable)
                    {
                        okCount++;
                    }
                    else
                    {
                        if (isDetectedByNetwork)
                        {
                            fpCount++;
                        }
                        else
                        {
                            fnCount++;
                        }
                    }

                    count++;

                    reset.Reset();
                }

                double percent = ((double)okCount / count) * 100.0;

                var result = new TestResult(percent, fnCount, fpCount);
                Console.WriteLine(result);
            });
        }
    }
}
