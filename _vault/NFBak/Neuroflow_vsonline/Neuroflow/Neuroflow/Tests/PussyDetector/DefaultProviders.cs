using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using PussyDetector.Properties;
using Neuroflow.Networks.Neural;

namespace PussyDetector
{
    public static class DefaultProviders
    {
        public static IUnorderedNeuralVectorFlowProvider CreateFullSampleTrainingProvider(int sampleSize, int? subSampleSize = null, int numberOfComputationIterations = 1)
        {
            Contract.Requires(numberOfComputationIterations > 0);
            Contract.Requires(sampleSize > 0);

            return new FullSampleProvider(DefaultSets.GetTrainingSet(), sampleSize, subSampleSize, numberOfComputationIterations);
        }

        public static IUnorderedNeuralVectorFlowProvider CreateFullSampleValidationProvider(int sampleSize, int? subSampleSize = null, int numberOfComputationIterations = 1)
        {
            Contract.Requires(numberOfComputationIterations > 0);
            Contract.Requires(sampleSize > 0);

            return new FullSampleProvider(DefaultSets.GetValidationSet(), sampleSize, subSampleSize, numberOfComputationIterations);
        }
    }
}
