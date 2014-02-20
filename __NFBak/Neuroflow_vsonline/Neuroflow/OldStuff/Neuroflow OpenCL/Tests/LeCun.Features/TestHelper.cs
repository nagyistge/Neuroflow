using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics;

namespace LeCun.Features
{
    public static class TestHelper
    {
        static List<NeuralVectorFlow> validationVectorFlows;
        
        public static void ShowPercent(NeuralNetwork network, IUnorderedNeuralVectorFlowProvider provider)
        {
            Contract.Requires(network != null);
            Contract.Requires(provider != null);

            if (!Console.KeyAvailable) return;

            Console.ReadKey();

            var prov = provider as LeCunSampleProvider;
            if (prov == null) return;

            if (validationVectorFlows == null) validationVectorFlows = prov.GetAll().ToList();

            var clone = network; // TODO: Clone

            double count = 0;
            double okCount = 0;

            foreach (var vectorFlow in validationVectorFlows)
            {
                clone.Reset();

                var entry = vectorFlow.Entries[0];

                for (int idx = 0; idx < entry.InputVector.Length; idx++)
                {
                    clone.InputInterface[idx] = entry.InputVector[idx].Value;
                }

                for (int i = 0; i < entry.NumberOfIterations; i++)
                {
                    clone.Iteration();
                }

                int desiredNumber = GetNumber(entry.DesiredOutputVector);
                int detectedNumber = GetNumber(clone.OutputInterface);

                if (desiredNumber == detectedNumber) okCount++;

                count++;
            }

            double current = 100.0 - (okCount / count) * 100.0;

            Console.WriteLine("Current precent: " + current.ToString("0.0000") + "%");

            GC.Collect();
        }

        private static int GetNumber(ComputationInterface<double> intf)
        {
            double[] intfValues = new double[intf.Length];
            for (int idx = 0; idx < intfValues.Length; idx++)
            {
                intfValues[idx] = intf.FastGet(idx);
            }
            return GetNumber(intfValues);
        }

        private static int GetNumber(double?[] vector)
        {
            return GetNumber(vector.Select(v => v.Value));
        }

        private static int GetNumber(IEnumerable<double> vectorValues)
        {
            int highIdx = -1;
            double high = double.MinValue;
            int idx = 0;
            foreach (var v in vectorValues)
            {
                if (v > high)
                {
                    high = v;
                    highIdx = idx;
                }
                idx++;
            }

            Debug.Assert(highIdx != -1);

            return highIdx;
        }
    }
}
