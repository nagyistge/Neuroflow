using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics;
using NeoComp.Networks;
using NeoComp.Optimization.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Algorithms.Quantum;
using NeoComp.Core;
using NeoComp.Computations;
using NeoComp.Adjustables;

namespace TEST.QA
{
    public static class QuantumAnnealing
    {
        const string problem =
            @"
            -5.0 = 25.0
            -4.0 = 16.0
            -3.0 = 9.0
            -2.0 = 4.0
            -1.0 = 1.0
            0.0 = 0.0
            1.0 = 1.0
            2.0 = 4.0
            3.0 = 9.0
            4.0 = 16.0
            5.0 = 25.0
            ";

//@"
//0.0 0.0 0.0 = 0.0 0.0
//0.0 0.0 1.0 = 0.0 1.0
//0.0 1.0 0.0 = 0.0 1.0
//0.0 1.0 1.0 = 1.0 0.0
//1.0 0.0 0.0 = 0.0 1.0
//1.0 0.0 1.0 = 1.0 0.0
//1.0 1.0 0.0 = 1.0 0.0
//1.0 1.0 1.0 = 1.0 1.0
//";

        public static void Begin()
        {
            var test = NeuralNetworkTest.Create(problem);

            //var network = NeuralArchitecture.CreateLayered(
            //               test.InputSize,
            //               test.OutputSize,
            //               () => new Synapse(),
            //               () => new ActivationNeuron(new LinearActivationFunction(1)),
            //               true,
            //               20);

            var network = NeuralArchitecture.CreateFullConnected(
                           test.InputSize,
                           test.OutputSize,
                           200,
                           () => new Synapse(),
                           () => new ActivationNeuron(new LinearActivationFunction(1)),
                           true);

            var sw = new Stopwatch();
            double errorTreshold = 0.00000000;
            int iteration = 0;
            double currentError = test.Test(network).MSE;
            var items = network.GetAdjustableItems().Select(i => (IQuantumStatedItem)new QSAItem(i)).ToArray();

            foreach (var item in items) item.State = 0.5;

            var prevItems = items.Select(i => i.State).ToList();

            double acceptTreshold = 0.0;

            double acceptTresholdInc = Math.Min(Math.Pow(10.0, -Math.Log(items.Length)) * 1.5, 0.0001);

            Console.WriteLine("Count: {0}, Inc: {1}", items.Length, acceptTresholdInc);

            double mseSplitPoint = Math.Min(1.0, 1.0 / ((double)items.Length) * 1000.0);

            Console.WriteLine(mseSplitPoint);
            
            //return;

            //double strength = Math.Sqrt(currentError);

            do
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey();
                    break;
                }

                sw.Start();
                
                double strength = currentError * (1.0 - mseSplitPoint) + Math.Sqrt(currentError) * mseSplitPoint;
                Tunneling(items, strength);
                double newError = test.Test(network).MSE;
                if (newError < currentError ||
                    RandomGenerator.Random.NextDouble() < acceptTreshold * (1.0 - newError))
                {
                    currentError = newError;
                    Save(prevItems, items);
                    acceptTreshold = 0.0;
                }
                else
                {
                    Restore(items, prevItems);
                    acceptTreshold += acceptTresholdInc;
                }

                sw.Stop();

                iteration++;

                //currentError = newError;
                if (iteration % 10 == 0)
                {
                    //Console.WriteLine(1.0 - newError);
                    Console.WriteLine("MSE: {0} Time: {1} Iteration: {2}",
                        currentError.ToString(),
                        sw.Elapsed.TotalSeconds.ToString("0.00"),
                        iteration);
                }
            }
            while (currentError > errorTreshold);
        }

        private static void Save(List<QuantumState> prevItems, IQuantumStatedItem[] items)
        {
            for (int idx = 0; idx < items.Length; idx++) prevItems[idx] = items[idx].State;
        }

        private static void Restore(IQuantumStatedItem[] items, List<QuantumState> prevItems)
        {
            for (int idx = 0; idx < items.Length; idx++) items[idx].State = prevItems[idx];
        }

        private static void Tunneling(IQuantumStatedItem[] items, double strength)
        {
            double s2 = strength / 2.0;
            foreach (var item in items)
            {
                double noise = (RandomGenerator.Random.NextDouble() * strength) - s2;
                item.State += noise;
            }
        }

        private static double GetTunnelingFieldStrength(double mse)
        {
            //return mse;
            return Math.Sqrt(mse);
        }
    }

    internal sealed class QSAItem : IQuantumStatedItem
    {
        internal QSAItem(IAdjustableItem item)
        {
            Contract.Requires(item != null);

            this.item = item;
        }

        IAdjustableItem item;

        QuantumState IQuantumStatedItem.State
        {
            get { return (item.Adjustment + 1.0) / 2.0; }
            set { item.Adjustment = value * 2.0 - 1.0; }
        }
    }
}
