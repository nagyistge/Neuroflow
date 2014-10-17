using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Architectures;
using System.ComponentModel;
using System.Activities;
using System.Collections.ObjectModel;
using NeoComp.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks
{
    public abstract class NeuralNetworkBlueprint<T> : Blueprint<NeuralNetwork>
        where T : INeuralArchitecture
    {
        public string VariableName { get; set; }
        
        [Category(PropertyCategories.Architecture)]
        public InArgument<int> InputInterfaceLength { get; set; }

        [Category(PropertyCategories.Architecture)]
        public InArgument<int> OutputInterfaceLength { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (InputInterfaceLength == null) metadata.AddValidationError("Input Interface Length is expected.");
            if (OutputInterfaceLength == null) metadata.AddValidationError("Output Interface Length is expected.");

            base.CacheMetadata(metadata);
        }

        protected override NeuralNetwork CreateObject(NativeActivityContext context)
        {
            var arch = CreateNeuralArchitecture(context, InputInterfaceLength.Get(context), OutputInterfaceLength.Get(context));
            return arch.CreateNetwork();
        }

        protected abstract T CreateNeuralArchitecture(System.Activities.NativeActivityContext context, int inputInterfaceLength, int outputInterfaceLength);
    }
}
