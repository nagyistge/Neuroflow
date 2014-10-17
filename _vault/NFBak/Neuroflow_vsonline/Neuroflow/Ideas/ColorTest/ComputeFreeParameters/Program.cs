using ColorTest;
using Neuroflow.Core;
using Neuroflow.Core.Algorithms.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace ComputeFreeParameters
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunGA();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:\n" + ex);
                Console.ReadKey();
            }
        }

        private static void RunGA()
        {
            const int validationSampleCount = 3;

            var recipes = new ColorRecipes(@"recipe.xml");
            int trainingSampleCount = recipes.Count - validationSampleCount;

            var shuffled = recipes;//.OrderBy(r => RandomGenerator.Random.Next()).ToList();

            var trainingComp = new ColorMixerGenomComparer(shuffled.Take(trainingSampleCount));
            var validationComp = new ColorMixerGenomComparer(shuffled.Skip(trainingSampleCount));
            var bestComp = new CombinedColorMixerGenomComprarer(trainingComp, validationComp);

            // Algo
            int valuesCount = new ColorMixingPars().ValuesCount + new ColorFilteringPars().ValuesCount;

            // Cross Entropy
            var selAlgo = new GaussianSelectionAlgorithm(.15);
            var mp = new PointMutationPars(PointMutationType.Uniform, 0.005, 0.09);
            var opt = new CrossEntropy(valuesCount, 200, 40, mp, selAlgo, trainingComp);

            //// GA
            //var selAlgo = new GaussianSelectionAlgorithm(.6);
            ////var selAlgo = new TournamentSelectionAlgorithm(10);
            //var mp = new PointMutationPars(PointMutationType.Gaussian, 0.005, 0.025);
            //var opt = new GA(valuesCount, 200, 10, mp, selAlgo, trainingComp);

            // Harmony
            //var wsAlgo = new GaussianSelectionAlgorithm(.1, SelectionDirection.FromBottom);
            //var mp = new PointMutationPars(PointMutationType.Gaussian, 0.02, 0.005);
            //var opt = new HarmonySearch(valuesCount, 200, 99.9, mp, wsAlgo, trainingComp);
            
            
            opt.Initialize();

            Genom best = null;

            // Run
            while (true)
            {
                opt.NextGeneration();

                var current = opt[0];

                bool bestChanged = false;
                if (best == null)
                {
                    best = current;
                    bestChanged = true;
                }
                else if (bestComp.Compare(current, best) < 0)
                {
                    best = current;
                    bestChanged = true;
                }

                //if (opt.GenerationNo % opt.PopulationSize == 0)
                {
                    Console.WriteLine("\n-- Generation {0} --", opt.GenerationNo);
                    Show("Current", current, trainingComp, validationComp);
                    Show("Best", best, trainingComp, validationComp, bestChanged);
                }
            }
        }

        private static void Show(string title, Genom genom, GeneBasedColorMixer trainingMixer, GeneBasedColorMixer validationMixer, bool store = false)
        {
            var trainingResult = trainingMixer.CreateMixedColors(genom);
            var validationResult = validationMixer.CreateMixedColors(genom);

            Console.WriteLine("\n-- {0} --", title);

            Console.WriteLine("Training Errors - Max: {0}, Avg: {1}, Min: {2}", trainingResult.MaxError.ToString("0.0000"), trainingResult.AvgError.ToString("0.0000"), trainingResult.MinError.ToString("0.0000"));
            Console.WriteLine("Training Worst Mix: {0}", trainingResult.WorstMix);
            Console.WriteLine("Training Best Mix: {0}", trainingResult.BestMix);
            Console.WriteLine();
            Console.WriteLine("Validation Errors - Max: {0}, Avg: {1}, Min: {2}", validationResult.MaxError.ToString("0.0000"), validationResult.AvgError.ToString("0.0000"), validationResult.MinError.ToString("0.0000"));
            Console.WriteLine("Validation Worst Mix: {0}", validationResult.WorstMix);
            Console.WriteLine("Validation Best Mix: {0}", validationResult.BestMix);

            if (store)
            {
                ColorFilteringPars fp;
                ColorMixingPars mp;
                GeneBasedColorMixer.ToColorMixerPars(genom, out fp, out mp);
                foreach (var bestFiles in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "best*.*")) File.Delete(bestFiles);
                string fileName = "best-t-" + trainingResult.WorstMix.Error.ToString("0.0000") + "-v-" + validationResult.WorstMix.Error.ToString("0.0000");
                using (var ffs = File.CreateText(fileName + ".fp.xml"))
                using (var fw = XmlWriter.Create(ffs, new XmlWriterSettings { Indent = true }))
                using (var mfs = File.CreateText(fileName + ".mp.xml"))
                using (var mw = XmlWriter.Create(mfs, new XmlWriterSettings { Indent = true }))
                {
                    //new DataContractSerializer(typeof(ColorFilteringPars)).WriteObject(fw, fp);
                    //fw.Flush();
                    //new DataContractSerializer(typeof(ColorMixingPars)).WriteObject(mw, mp);
                    //mw.Flush();
                }
            }
        }
    }
}
