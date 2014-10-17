using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    [Flags]
    public enum NNAlgorithm
    {
        None = 0,
        Validation = 1,
        UnorderedTraining = 2,
        StreamedTraining = 4,
        UnorderedTrainingAndValidation = Validation | UnorderedTraining,
        StreamedTrainingAndValidation = Validation | StreamedTraining,
        All = Validation | UnorderedTraining | StreamedTraining
    }
}
