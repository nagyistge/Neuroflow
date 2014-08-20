using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimizations;
using System.Runtime.Serialization;
using NeoComp.Threading;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.NeuralNetworks.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "neuralBatchExec")]
    public abstract class NeuralBatchExecution : VectorFlowBatchExecution<double>, ISynchronized
    {
        public NeuralBatchExecution(NeuralNetwork network, IterationRepeatPars repeatPars = null)
            : base((IComputationUnit<double>)network, 0.5, repeatPars)
        {
            Contract.Requires(network != null);
        }

        new public NeuralNetwork ComputationUnit
        {
            get { return (NeuralNetwork)base.ComputationUnit; }
        }

        public NeuralNetwork Network
        {
            get { return ComputationUnit; }
        }

        public SyncContext SyncRoot
        {
            get { return Network.SyncRoot; }
        }

        protected override double ComputeError(double?[] desiredOutputVector)
        {
            double count = 0.0;
            double sum = 0.0;
            for (int idx = 0; idx < desiredOutputVector.Length; idx++)
            {
                double? desiredValue = desiredOutputVector[idx];
                if (desiredValue.HasValue)
                {
                    double difference = desiredValue.Value - ComputationUnit.OutputInterface[idx];
                    RegiterErrorDifference(idx, desiredValue.Value, ComputationUnit.OutputInterface[idx]);
                    sum += Math.Pow(difference * ErrorScale, 2.0);
                    count++;
                }
            }
            return count != 0.0 ? (sum / count) / 2.0 : 0.0;
        }

        protected virtual void RegiterErrorDifference(int index, double desiredValue, double currentValue)
        {
            Contract.Requires(index >= 0 && index < ComputationUnit.OutputInterface.Length);
        }
    }
}
