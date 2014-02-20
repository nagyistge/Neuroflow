using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using PussyDetector.Properties;
using System.Drawing;
using Neuroflow.Core.Optimizations.NeuralNetworks;

namespace PussyDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoStuff();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void DoStuff()
        {
            var ts = DefaultSets.GetTrainingSet();
            var vs = DefaultSets.GetValidationSet();

            var tp = new FullSampleProvider(ts, Settings.Default.SampleSize);

            var idx = new IndexSet(new[] { 1, 4, 6, 7, 19 });

            var nv = tp.GetNextVectors(idx);
        }
    }
}
