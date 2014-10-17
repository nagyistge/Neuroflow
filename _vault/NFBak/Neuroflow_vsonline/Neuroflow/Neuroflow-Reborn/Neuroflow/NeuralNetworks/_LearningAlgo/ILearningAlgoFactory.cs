using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public interface ILearningAlgoFactory
    {
        ILearningAlgo CreateLearningAlgo(LearningBehavior learningBehavior, ReadOnlyCollection<TrainingNode> nodes);
    }
}
