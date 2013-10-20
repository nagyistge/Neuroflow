using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    internal abstract class ManagedLearningAlgo<TRule> : ILearningAlgo 
        where TRule : LearningBehavior
    {
        internal ManagedLearningAlgo(TRule rule, ReadOnlyCollection<TrainingNode> nodes)
        {
            Debug.Assert(rule != null);
            Debug.Assert(nodes != null && nodes.Count > 0);

            Rule = rule;
            Nodes = nodes;
        }

        public abstract LearningAlgoIterationType IterationTypes { get; }

        protected TRule Rule { get; private set; }

        protected ReadOnlyCollection<TrainingNode> Nodes { get; private set; }

        protected abstract void Run(int iterationCount, IDeviceArray error);

        protected virtual void Initialize() { }

        void ILearningAlgo.Run(int iterationCount, IDeviceArray error)
        {
            Run(iterationCount, error);
        }

        void ILearningAlgo.Initialize()
        {
            Initialize();
        }
    }
}
