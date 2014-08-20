using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.ComponentModel;
using System.Activities;
using NeoComp.NeuralNetworks.ActivationFunctions;

namespace NeoComp.Activities.NeuralNetworks.ActivationFunctions
{
    public abstract class ActivationFunctionBlueprint<T> : Blueprint<IActivationFunction>
    {
        [Category(PropertyCategories.Math)]
        [DefaultExpression("1.05")]
        public InArgument<double> Alpha { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Alpha == null) metadata.AddValidationError("Alpha is expected.");

            base.CacheMetadata(metadata);
        }
        
        protected override IActivationFunction CreateObject(System.Activities.NativeActivityContext context)
        {
            var af = CreateActivationFunction(context);
            af.Alpha = Alpha.Get(context);
            return af;
        }

        protected abstract IActivationFunction CreateActivationFunction(System.Activities.NativeActivityContext context);
    }
}
