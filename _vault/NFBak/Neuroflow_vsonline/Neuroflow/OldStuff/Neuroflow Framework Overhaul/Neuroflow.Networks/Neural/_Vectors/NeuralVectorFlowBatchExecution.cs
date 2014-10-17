using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks.Neural
{
    [Serializable]
    public abstract class NeuralVectorFlowBatchExecution : VectorFlowBatchExecution<double>
    {
        const double ErrorScale = 0.5;
        
        protected NeuralVectorFlowBatchExecution(NeuralNetwork network, IterationRepeatPars repeatPars = null)
            : base(network, repeatPars)
        {
            Contract.Requires(network != null);
        }

        public NeuralNetwork Network
        {
            get { return (NeuralNetwork)ComputationUnit; }
        }

        protected override double? ComputeError(double?[] desiredOutputVector)
        {
            double count = 0.0;
            double sum = 0.0;
            for (int idx = 0; idx < desiredOutputVector.Length; idx++)
            {
                double? desiredValue = desiredOutputVector[idx];
                if (desiredValue.HasValue)
                {
                    double currentValue = ComputationUnit.OutputInterface.FastGet(idx);
                    double difference = desiredValue.Value - currentValue;
                    RegiterErrorDifference(idx, desiredValue.Value, currentValue);
                    sum += Math.Pow(difference * ErrorScale, 2.0);
                    count++;
                }
            }
            return count != 0.0 ? ((sum / count) / 2.0) : (double?)null;
        }

        protected virtual void RegiterErrorDifference(int index, double desiredValue, double currentValue)
        {
            Contract.Requires(index >= 0 && index < ComputationUnit.OutputInterface.Length);
        }

        unsafe protected override void WriteInputVector(double?[] inputVector)
        {
            ComputationUnit.InputInterface.FastWrite(inputVector);
        }
    }
}
