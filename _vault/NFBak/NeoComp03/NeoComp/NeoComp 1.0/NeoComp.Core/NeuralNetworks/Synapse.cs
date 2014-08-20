using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Runtime.Serialization;
using NeoComp.ComputationalNetworks;
using NeoComp.Collections;
using NeoComp.NeuralNetworks.Learning;

namespace NeoComp.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "synapse")]
    public class Synapse : ComputationConnection<double>, IBackwardConnection, ILearningConnection
    {
        public Synapse(params ILearningRule[] learningRules)
        {
            if (learningRules != null)
            {
                var fa = learningRules.ToArray();
                if (fa.Length != 0) this.learningRules = ReadOnlyArray.Wrap(fa);
            }
        }

        [DataMember(Name = "weight")]
        public double Weight { get; set; }

        public sealed override double OutputValue
        {
            get { return InputValue * Weight; }
        }

        [NonSerialized]
        ReadOnlyArray<ILearningRule> learningRules;

        public ReadOnlyArray<ILearningRule> LearningRules
        {
            get { return learningRules; }
        }

        [NonSerialized]
        BackwardValues backwardValues = new BackwardValues();

        BackwardValues IBackwardConnection.BackwardValues
        {
            get { return backwardValues; }
        }

        IEnumerable<ILearningRule> ILearningConnection.LearningRules
        {
            get { return LearningRules; }
        }
    }
}
