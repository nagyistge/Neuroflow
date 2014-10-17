using Neuroflow.Core.ComponentModel;
using Neuroflow.Core.Vectors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public abstract class NeuralNetworkFactory
    {
        [DisplayName("Recurrent Options")]
        [Category(PropertyCategories.Structure)]
        public RecurrentOptions RecurrentOptions { get; set; }

        public abstract NeuralComputationContext CreateContext();

        public abstract VectorBuffer<float> CreateVectorBuffer();

        public abstract IBufferOps CreateBufferOps();

        public abstract IFeedForwardOps CreateFeedForwardOps();

        public abstract IBPTTOps CreateBPTTOps();

        public abstract IRTLROps CreateRTLROps();
    }
}
