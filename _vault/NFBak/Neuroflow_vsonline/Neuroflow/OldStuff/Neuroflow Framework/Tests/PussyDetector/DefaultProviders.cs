using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.Diagnostics.Contracts;
using PussyDetector.Properties;

namespace PussyDetector
{
    public static class DefaultProviders
    {
        public static IUnorderedNeuralVectorsProvider CreateFullSampleTrainingProvider(int sampleSize, int? subSampleSize = null, int numberOfComputationIterations = 1)
        {
            Contract.Requires(numberOfComputationIterations > 0);
            Contract.Requires(sampleSize > 0);

            return new FullSampleProvider(DefaultSets.GetTrainingSet(), sampleSize, subSampleSize, numberOfComputationIterations);
        }

        public static IUnorderedNeuralVectorsProvider CreateFullSampleValidationProvider(int sampleSize, int? subSampleSize = null, int numberOfComputationIterations = 1)
        {
            Contract.Requires(numberOfComputationIterations > 0);
            Contract.Requires(sampleSize > 0);

            return new FullSampleProvider(DefaultSets.GetValidationSet(), sampleSize, subSampleSize, numberOfComputationIterations);
        }
    }
}
