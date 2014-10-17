using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    internal class ManagedGradientDescentLearningAlgo : ManagedLearningAlgo<GradientDescentLearningRule>
    {
        internal ManagedGradientDescentLearningAlgo(GradientDescentLearningRule rule, ReadOnlyCollection<TrainingNode> nodes) :
            base(rule, nodes)
        {
            if (rule.WeightUpdateMode == WeigthUpdateMode.Online)
            {
                onlineCode = new List<Action>();
            }
            else
            {
                offlineCode = new List<Action<int>>();
            }

            foreach (var node in nodes)
            {
                var weights = node.Weights;
                var gradients = rule.WeightUpdateMode == WeigthUpdateMode.Online ? node.Gradients : node.GradientSums;

                Debug.Assert(weights != null);
                Debug.Assert(gradients != null);

                Debug.Assert(weights.Count > 0);
                Debug.Assert(weights.Count == gradients.Count);

                for (int i = 0; i < weights.Count; i++)
                {
                    var wa = (ManagedArray)weights[i];
                    var ga = (ManagedArray)gradients[i];

                    Debug.Assert(wa.Size == ga.Size);

                    var lua = new ManagedArray(wa.Size);

                    if (rule.WeightUpdateMode == WeigthUpdateMode.Online)
                    {
                        onlineCode.Add(
                            () =>
                            {
                                ManagedComputeGradientDescent.UpdateWeightsOnline(
                                    lua,
                                    wa,
                                    ga,
                                    rule.LearningRate,
                                    rule.Momentum,
                                    rule.Smoothing);
                            });
                    }
                    else
                    {
                        offlineCode.Add(
                            (iterationCount) =>
                            {
                                ManagedComputeGradientDescent.UpdateWeightsOffline(
                                    lua,
                                    wa,
                                    ga,
                                    iterationCount,
                                    rule.LearningRate,
                                    rule.Momentum,
                                    rule.Smoothing);
                            });
                    }
                }        
            }
        }

        public override LearningAlgoIterationType IterationTypes
        {
            get { return Rule.WeightUpdateMode == WeigthUpdateMode.Online ? LearningAlgoIterationType.SupervisedOnline : LearningAlgoIterationType.SupervisedOffline; }
        }

        List<Action> onlineCode;

        List<Action<int>> offlineCode;

        protected override void Run(int iterationCount, IDeviceArray error)
        {
            if (onlineCode != null)
            {
                foreach (var c in onlineCode) c();
            }
            else
            {
                Debug.Assert(offlineCode != null);
                foreach (var c in offlineCode) c(iterationCount);
            }
        }
    }
}
