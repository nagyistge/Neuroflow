using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.Activities;
using NeoComp.NeuralNetworks.ActivationFunctions;

namespace NeoComp.Activities.NeuralNetworks.ActivationFunctions
{
    public sealed class LinearActivationFunctionBlueprint : ActivationFunctionBlueprint<LinearActivationFunction>
    {
        protected override IActivationFunction CreateActivationFunction(System.Activities.NativeActivityContext context)
        {
            return new LinearActivationFunction();
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new LinearActivationFunctionBlueprint { DisplayName = "Linear Activation Function" };
        }
    }
}
