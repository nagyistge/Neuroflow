using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(ISupervisedLearningEpochContract))]
    public interface ISupervisedLearningEpoch
    {
        LearningStrategy Strategy { get; }

        NeuralNetwork Network { get; }

        int Iteration { get; }

        NeuralNetworkTest Test { get; set; }

        void Initialize();

        void Reset();

        NeuralNetworkTestResult Step();
    }

    [ContractClassFor(typeof(ISupervisedLearningEpoch))]
    class ISupervisedLearningEpochContract : ISupervisedLearningEpoch
    {
        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            ISupervisedLearningEpoch e = this;
            Contract.Invariant(e.Network != null);
            Contract.Invariant(e.Strategy != null);
        }
        
        LearningStrategy ISupervisedLearningEpoch.Strategy
        {
            get { return null; }
        }

        NeuralNetwork ISupervisedLearningEpoch.Network
        {
            get { return null; }
        }

        int ISupervisedLearningEpoch.Iteration
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return 0;
            }
        }

        NeuralNetworkTest ISupervisedLearningEpoch.Test
        {
            get
            {
                return null;
            }
            set
            {
                Contract.Requires(value != null);
            }
        }

        void ISupervisedLearningEpoch.Initialize()
        {
        }

        void ISupervisedLearningEpoch.Reset()
        {
        }

        NeuralNetworkTestResult ISupervisedLearningEpoch.Step()
        {
            Contract.Ensures(Contract.Result<NeuralNetworkTestResult>() != null);
            return null;
        }
    }

}
