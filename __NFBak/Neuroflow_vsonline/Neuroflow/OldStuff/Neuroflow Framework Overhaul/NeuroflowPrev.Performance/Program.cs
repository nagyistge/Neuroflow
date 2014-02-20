using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks.Architectures;
using Neuroflow.Core;
using Neuroflow.Core.NeuralNetworks;
using Neuroflow.Core.NeuralNetworks.ActivationFunctions;
using Neuroflow.Core.ComputationalNetworks;
using System.Diagnostics;
using Neuroflow.Core.NeuralNetworks.Learning;

namespace NeuroflowPrev.Performance
{
    class Program
    {
        const int InputLength = 100;
        const int OutputLength = 100;
        const int NodeCount = 100;

        const int TestIterationCount = 100000;

        static void Main(string[] args)
        {
            var a = new WiredNeuralArchitecture(
                InputLength,
                OutputLength,
                NodeCount,
                new Factory<OperationNode<double>>(() => new ActivationNeuron(new SigmoidActivationFunction { Alpha = 1.05 })),
                new Factory<OperationNode<double>>(() => new ActivationNeuron(new LinearActivationFunction(1.05))),
                new Factory<ComputationConnection<double>>(() => new Synapse()));

            Console.WriteLine("Creating Wired Network ...");
            var net = a.CreateNetwork();
            Console.WriteLine("Done.");

            Console.WriteLine();
            Console.WriteLine("Begin test, number of iterations: {0}", TestIterationCount);
            var sw = new Stopwatch();

            var t = new UnorderedTraining(net);

            t.EnsureScriptRun();

            sw.Start();

            lock (net.SyncRoot)
            {
                for (int i = 0; i < TestIterationCount; i++)
                {
                    //net.Iteration();
                    t.BackwardIteration();
                }
            }

            sw.Stop();

            Console.WriteLine("Total Millisec: {0}", sw.ElapsedMilliseconds);

            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }
    }
}
