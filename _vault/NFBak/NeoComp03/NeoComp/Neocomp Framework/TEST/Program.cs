using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;
using System.Threading;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            //TSP.TSPQSATest.Begin();
            //TSP.TSPGATest.Begin();

            //Logic.LogicGATest.Begin();
            //Neural.NeuralGATest.Begin();

            //Neural.AdjustedGATest.Begin();

            //Learning.Test.Begin();
            //QA.QuantumAnnealing.Begin();

            //AQATSP.TSP.Begin();

            //Supervised.Test.Begin();

            DataTransform.DataTest.Begin();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
