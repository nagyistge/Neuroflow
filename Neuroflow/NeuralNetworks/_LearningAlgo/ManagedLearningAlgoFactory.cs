using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using System.Collections.ObjectModel;

namespace Neuroflow.NeuralNetworks
{
    public sealed class ManagedLearningAlgoFactory : ILearningAlgoFactory
    {
        public ILearningAlgo CreateLearningAlgo(LearningBehavior learningBehavior, ReadOnlyCollection<TrainingNode> nodes)
        {
            if (learningBehavior is GradientDescentLearningRule) return CreateLearningAlgo<ManagedGradientDescentLearningAlgo>(learningBehavior, nodes);
            if (learningBehavior is CrossEntropyLearningRule) return CreateLearningAlgo<ManagedCrossEntropyLearningAlgo>(learningBehavior, nodes);
            if (learningBehavior is AlopexBLearningRule) return CreateLearningAlgo<ManagedAlopexBLearningAlgo>(learningBehavior, nodes);
            return null;
        }

        private ILearningAlgo CreateLearningAlgo<TAlgo>(LearningBehavior rule, ReadOnlyCollection<TrainingNode> nodes)
            where TAlgo : class, ILearningAlgo
        {
            return (ILearningAlgo)typeof(TAlgo).CreateInstance(rule, nodes);
        }
    }
}
