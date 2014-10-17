using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using PussyDetector.Properties;

namespace PussyDetector
{
    public static class DefaultSets
    {
        static object sync = new object();

        static List<FeatureStream> trainingSet;

        static List<FeatureStream> validationSet;

        public static IList<FeatureStream> GetTrainingSet()
        {
            EnsureInitialized();
            return trainingSet.AsReadOnly();
        }

        public static IList<FeatureStream> GetValidationSet()
        {
            EnsureInitialized();
            return validationSet.AsReadOnly();
        }

        static void EnsureInitialized()
        {
            if (trainingSet == null)
            {
                lock (sync)
                {
                    if (trainingSet == null)
                    {
                        Initialze();
                    }
                }
            }
        }

        private static void Initialze()
        {
            var nonObjImages = FileHelpers.SearchForNotPussyFiles().Select(fi => new FeatureStream(fi, false)).OrderByRandom().ToList();
            var objImages = FileHelpers.SearchForPussyFiles().Select(fi => new FeatureStream(fi, true)).OrderByRandom().ToList();

            var trainingSet1 = new List<FeatureStream>();
            var validationSet1 = new List<FeatureStream>();

            int setSize = nonObjImages.Count;

            int validationSetSize = (int)Math.Round(((float)Settings.Default.ValidationPercent / 100.0) * setSize);
            int trainingSetSize = setSize - validationSetSize;

            foreach (var stream in nonObjImages)
            {
                if (trainingSet1.Count != trainingSetSize * 2)
                {
                    trainingSet1.Add(stream);
                    trainingSet1.Add(stream.GetReversed());
                }
                else
                {
                    validationSet1.Add(stream);
                    validationSet1.Add(stream.GetReversed());
                }
            }

            var trainingSet2 = new List<FeatureStream>();
            var validationSet2 = new List<FeatureStream>();

            setSize = objImages.Count;

            validationSetSize = (int)Math.Round(((float)Settings.Default.ValidationPercent / 100.0) * setSize);
            trainingSetSize = setSize - validationSetSize;

            foreach (var stream in objImages)
            {
                if (trainingSet2.Count != trainingSetSize * 2)
                {
                    trainingSet2.Add(stream);
                    trainingSet2.Add(stream.GetReversed());
                }
                else
                {
                    validationSet2.Add(stream);
                    validationSet2.Add(stream.GetReversed());
                }
            }

            trainingSet = new List<FeatureStream>(trainingSet1.Concat(trainingSet2).OrderByRandom());
            validationSet = new List<FeatureStream>(validationSet1.Concat(validationSet2).OrderByRandom());
        }
    }
}
