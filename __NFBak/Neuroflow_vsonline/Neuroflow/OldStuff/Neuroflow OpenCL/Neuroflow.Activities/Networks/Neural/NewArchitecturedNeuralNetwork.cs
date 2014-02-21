using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public abstract class NewArchitecturedNeuralNetwork<T> : NewObjectActivity<NeuralNetwork>
        where T : NeuralNetworkArchitecture
    {
        public NewArchitecturedNeuralNetwork()
            : base()
        {
            DisplayName = "Neural Network";
        }

        protected override Type ObjectType
        {
            get { return typeof(T); }
        }

        protected override void ValuesCreated(System.Activities.NativeActivityContext context)
        {
            var values = CreatedValues.Get(context);
            var propValues = GetPropertyValues();
            var architecture = NewObjectActivityHelpers.CreateObject<T>(ObjectType, propValues, values);
            Result.Set(context, architecture.CreateNetwork());
        }
    }
}
