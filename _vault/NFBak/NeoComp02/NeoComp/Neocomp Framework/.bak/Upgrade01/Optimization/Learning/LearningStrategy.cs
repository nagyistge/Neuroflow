using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;

namespace NeoComp.Optimization.Learning
{
    public abstract class LearningStrategy : SynchronizedObject
    {
        protected LearningStrategy(NeuralNetwork network)
        {
            Contract.Requires(network != null);

            Network = network;
        }
        
        public NeuralNetwork Network { get; private set; }

        protected internal abstract void InitializeNewRun();
    }
}
