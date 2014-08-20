using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;
using Neuroflow.Core;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neuroflow.Performance
{
    class Program
    {
        const int InputLength = 100;
        const int OutputLength = 100;
        const int NodeCount = 100;
        const int LayerCount = 2;

        static readonly Factory<NeuralNode> NodeSF = new Factory<NeuralNode>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05)));
        static readonly Factory<NeuralNode> NodeLF = new Factory<NeuralNode>(() => new ActivationNeuron(new LinearActivationFunction(1.05)));
        static readonly Factory<NeuralConnection> ConnF = new Factory<NeuralConnection>(() => new Synapse());

        const int TestIterationCount = 10000;
        
        static void Main(string[] args)
        {
            TestMLA();

            Console.WriteLine();

            TestWired();

            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }

        private static void TestMLA()
        {
            var layers = new NeuralLayerDefinition[LayerCount];

            for (int i = 0; i < LayerCount; i++)
            {
                if (i < (LayerCount - 1))
                {
                    layers[i] = new NeuralLayerDefinition(NodeSF, NodeCount);
                }
                else
                {
                    layers[i] = new NeuralLayerDefinition(NodeLF, OutputLength);
                }
            }

            var mla = new StandardMultilayerArchitecture(new NeuralConnectionDefinition(ConnF, false), InputLength, layers);

            Console.WriteLine("Creating MLA Network ...");
            var net = mla.CreateNetwork();
            Console.WriteLine("Done.");

            Console.WriteLine();
            Console.WriteLine("Begin test, number of iterations: {0}", TestIterationCount);
            var sw = new Stopwatch();

            sw.Start();

            lock (net.SyncRoot)
            {
                for (int i = 0; i < TestIterationCount; i++)
                {
                    net.LockedIteration();
                    net.LockedBackwardIteration(BackwardComputationMode.FeedForward);
                    net.LockedReset();
                    net.LockedResetGradientSums();
                }
            }

            sw.Stop();

            Console.WriteLine("Total Millisec: {0}", sw.ElapsedMilliseconds);
        }

        private static void TestWired()
        {
            var wa = new WiredArchitecture(InputLength, OutputLength, NodeCount, NodeSF, NodeLF, ConnF);

            Console.WriteLine("Creating Wired Network ...");
            var net = wa.CreateNetwork();
            Console.WriteLine("Done.");

            Console.WriteLine();
            Console.WriteLine("Begin test, number of iterations: {0}", TestIterationCount);
            var sw = new Stopwatch();

            sw.Start();

            lock (net.SyncRoot)
            {
                for (int i = 0; i < TestIterationCount; i++)
                {
                    net.LockedIteration();
                    net.LockedBackwardIteration(BackwardComputationMode.FeedForward);
                    net.LockedReset();
                    net.LockedResetGradientSums();
                }
            }

            sw.Stop();

            Console.WriteLine("Total Millisec: {0}", sw.ElapsedMilliseconds);
        }
    }
}
