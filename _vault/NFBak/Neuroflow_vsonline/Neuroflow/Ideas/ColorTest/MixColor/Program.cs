using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Media;
using System.Xml;
using ColorTest;
using Neuroflow.Core.Algorithms.Selection;

namespace MixColor
{
    class Program
    {
        static BaseColor[] baseColors =
        {
            new BaseColor("Black", new HdrRGB(68,58,59)),
            new BaseColor("Green", new HdrRGB(97,138,101)),
            new BaseColor("Blue", new HdrRGB(59,107,165)),
            new BaseColor("Red", new HdrRGB(186,67,97)),
            new BaseColor("Yellow", new HdrRGB(234,190,87)),
            new BaseColor("White", new HdrRGB(213,198,185)),
        };

        static void Main(string[] args)
        {
            try
            {
                Mix();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:\n" + ex);
                Console.ReadLine();
            }
        }

        static void Mix()
        {
            var target = new HdrRGB(158, 46, 123);
            var fp = LoadFilteringPars();
            var mp = LoadMixingPars();
            var comp = new ColorComponentSearchComparer(fp, mp, target, baseColors);

            //var selAlgo = new GaussianSelectionAlgorithm();
            var selAlgo = new TournamentSelectionAlgorithm(10);
            var mutPars = new PointMutationPars(PointMutationType.Gaussian, 0.01, 0.1);
            var opt = new GA(baseColors.Length, 200, 5, mutPars, selAlgo, comp);

            //var selAlgo = new GaussianSelectionAlgorithm(.15);
            //var mutPars = new PointMutationPars(PointMutationType.Gaussian, 0.005, 0.1);
            //var opt = new CrossEntropy(baseColors.Length, 200, 40, mutPars, selAlgo, comp);

            opt.Initialize();

            ColorComponentSearchResult best = null;

            while (true)
            {
                opt.NextGeneration();

                var cBest = opt[0];
                var current = (ColorComponentSearchResult)cBest.Data;

                if (best == null)
                {
                    best = current;
                }
                else if (current.CompareTo(best) < 0)
                {
                    best = current;
                }

                Console.WriteLine("\nCurrent: " + current);
                Console.WriteLine("         " + current.FinalResult);
                Console.WriteLine("\nBest   : " + best);
                Console.WriteLine("         " + best.FinalResult);
            }
        }

        static ColorFilteringPars LoadFilteringPars()
        {
            return LoadPars<ColorFilteringPars>("fp");
        }

        static ColorMixingPars LoadMixingPars()
        {
            return LoadPars<ColorMixingPars>("mp");
        }

        static T LoadPars<T>(string ext) where T : class
        {
            var fn = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*." + ext + ".xml").FirstOrDefault();
            if (fn == null) throw new FileNotFoundException(typeof(T).Name + " not found.");
            using (var fs = File.OpenText(fn))
            using (var r = XmlReader.Create(fs))
            {
                var fp = new DataContractSerializer(typeof(T)).ReadObject(r) as T;
                if (fp == null) throw new InvalidDataException(typeof(T).Name + " is unreadable.");
                return fp;
            }
        }
    }
}
