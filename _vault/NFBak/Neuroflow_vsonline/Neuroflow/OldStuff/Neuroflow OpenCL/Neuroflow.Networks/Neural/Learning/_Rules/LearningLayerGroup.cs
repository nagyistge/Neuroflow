using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class LearningLayerGroup
    {
        public LearningRule Rule { get; internal set; }

        public ReadOnlyCollection<ConnectedLayer> ConnectedLayers { get; internal set; }
    }
}
