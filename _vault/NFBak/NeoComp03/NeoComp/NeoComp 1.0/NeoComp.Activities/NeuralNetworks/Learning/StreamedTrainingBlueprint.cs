using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Learning;
using System.Activities;
using NeoComp.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public sealed class StreamedTrainingBlueprint : TrainingBlueprint
    {
        protected override Training CreateObject(NativeActivityContext context)
        {
            var network = GetFuncResult<NeuralNetwork>(context, "NeuralNetwork");

            return Training.CreateStreamed(network);
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new StreamedTrainingBlueprint { DisplayName = "Streamed Training" };
        }
    }
}
