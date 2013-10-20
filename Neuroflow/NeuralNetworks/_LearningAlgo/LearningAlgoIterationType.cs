using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    [Flags]
    public enum LearningAlgoIterationType
    {
        SupervisedOnline = 1 << 1,
        SupervisedOffline = 1 << 2
    }
}
