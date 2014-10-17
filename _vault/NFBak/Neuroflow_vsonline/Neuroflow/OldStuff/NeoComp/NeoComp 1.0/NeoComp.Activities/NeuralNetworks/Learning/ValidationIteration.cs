using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public sealed class ValidationIteration : TrainingIteration
    {
        internal override NeoComp.NeuralNetworks.Learning.NeuralBatchExecution GetExec(System.Activities.NativeActivityContext context)
        {
            var exec = (NeoComp.NeuralNetworks.Learning.Training)base.GetExec(context);
            return exec.CreateValidation();
        }
    }
}
