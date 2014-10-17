using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class TrainingNode
    {
        public TrainingNode(IEnumerable<IDeviceArray> weights, IEnumerable<IDeviceArray> gradients = null, IEnumerable<IDeviceArray> gradientSums = null)
        {
            Args.Requires(() => weights, () => weights != null);

            Weights = new ReadOnlyCollection<IDeviceArray>(weights.ToArray());
            if (gradients != null) Gradients = new ReadOnlyCollection<IDeviceArray>(gradients.ToArray());
            if (gradientSums != null) GradientSums = new ReadOnlyCollection<IDeviceArray>(gradientSums.ToArray());
        }

        public ReadOnlyCollection<IDeviceArray> Weights { get; private set; }

        public ReadOnlyCollection<IDeviceArray> Gradients { get; private set; }

        public ReadOnlyCollection<IDeviceArray> GradientSums { get; private set; }
    }
}
