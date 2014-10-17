using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using Neuroflow.Core.NeuralNetworks.Architectures;

namespace Neuroflow.Activities.NeuralNetworks
{
    public abstract class NewArchitecturedNeuralNetwork<T> : NewObjectActivity<NeuralNetwork>
        where T : class, INeuralArchitecture
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
