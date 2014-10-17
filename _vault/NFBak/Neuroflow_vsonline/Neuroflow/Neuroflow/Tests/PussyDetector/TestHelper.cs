using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Threading.Tasks;
using Neuroflow.Networks.Neural;

namespace PussyDetector
{
    public struct TestResult
    {
        public TestResult(float successPercent, int falseNegativeCount, int falsePositiveCount)
        {
            this.successPercent = successPercent;
            this.falseNegativeCount = falseNegativeCount;
            this.falsePositiveCount = falsePositiveCount;
        }
        
        float successPercent;

        public float SuccessPercent
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
        static List<NeuralVectorFlow> validationVectorFlows;

        public static void TestNetwork(NeuralNetwork network, IUnorderedNeuralVectorFlowProvider validationProvider)
        {
            //Contract.Requires(network != null);

            //if (!Console.KeyAvailable) return;
            //Console.ReadKey();

            //var prov = (FullSampleProvider)validationProvider;

            //if (validationVectorFlows == null) validationVectorFlows = prov.GetAllVectors().ToList();

            //var clone = network; //TODO: Clone

            ////Task.Factory.StartNew(() =>
            ////{
            //    int fnCount = 0;
            //    int fpCount = 0;
            //    int okCount = 0;
            //    int count = 0;

            //    foreach (var vectorFlow in validationVectorFlows)
            //    {
            //        bool isDetectable = false;

            //        clone.Reset(NeuralNetworkResetTarget.Outputs);

            //        foreach (var vector in vectorFlow.Entries)
            //        {
            //            clone.WriteInput(vector.InputVector);

            //            for (int i = 0; i < vector.NumberOfIterations; i++)
            //            {
            //                clone.Iteration();
            //            }

            //            if (vector.DesiredOutputVector[0] != null)
            //            {
            //                isDetectable = vector.DesiredOutputVector[0].Value > 0.0;
            //            }
            //        }

            //        float[] outputVector = new float[1];
            //        clone.ReadOutput(outputVector);
            //        float detectedOutput = outputVector[0];

            //        bool isDetectedByNetwork = detectedOutput > 0.0;

            //        if (isDetectedByNetwork == isDetectable)
            //        {
            //            okCount++;
            //        }
            //        else
            //        {
            //            if (isDetectedByNetwork)
            //            {
            //                fpCount++;
            //            }
            //            else
            //            {
            //                fnCount++;
            //            }
            //        }

            //        count++;
            //    }

            //    float percent = ((float)okCount / (float)count) * 100.0f;

            //    var result = new TestResult(percent, fnCount, fpCount);
            //    Console.WriteLine(result);
            ////});
        }
    }
}
