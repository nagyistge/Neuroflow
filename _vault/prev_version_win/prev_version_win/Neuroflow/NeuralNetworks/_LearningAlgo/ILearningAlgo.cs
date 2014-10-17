using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public interface ILearningAlgo
    {
        LearningAlgoIterationType IterationTypes { get; }

        void Initialize();

        void Run(int iterationCount, IDeviceArray error);
    }
}
