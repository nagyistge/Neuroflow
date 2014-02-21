using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Runtime.Serialization;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.Collections;
using Neuroflow.Core.NeuralNetworks.Learning;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "synapse")]
    public class Synapse : ComputationConnection<double>, IBackwardConnection, ILearningConnection
    {
        public Synapse(
            [Category(PropertyCategories.Algorithm)]
            [FreeDisplayName("LearningRules")]
            params ILearningRule[] learningRules)
        {
            if (learningRules != null)
            {
                var fa = learningRules.ToArray();
                if (fa.Length != 0) this.learningRules = ReadOnlyArray.Wrap(fa);
            }
        }

        [DataMember(Name = "weight")]
        [Browsable(false)]
        public double Weight { get; set; }

        public sealed override double OutputValue
        {
            get { return InputValue * Weight; }
        }

        [NonSerialized]
        ReadOnlyArray<ILearningRule> learningRules;

        [Browsable(false)]
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
