using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NeoComp.Activities.Design;
using System.Activities;
using NeoComp.NeuralNetworks.Learning;
using NeoComp.Optimizations;
using NeoComp.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public sealed class UnorderedTrainingBlueprint : TrainingBlueprint
    {
        [Category(PropertyCategories.Behavior)]
        [DefaultExpression("1")]
        [Order(0)]
        public InArgument<int> MinIterations { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DefaultExpression("5")]
        [Order(1)]
        public InArgument<int> MaxIterations { get; set; }

        protected override Training CreateObject(NativeActivityContext context)
        {
            var min = MinIterations.Get(context);
            var max = MaxIterations.Get(context);
            IterationRepeatPars pars = null;
            if (min >= 1 && (max >= min))
            {
                pars = new IterationRepeatPars(min, max);
            }
            var network = GetFuncResult<NeuralNetwork>(context, "NeuralNetwork");
            return Training.CreateUnordered(network, pars);
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new UnorderedTrainingBlueprint { DisplayName = "Unordered Training" };
        }
    }
}
