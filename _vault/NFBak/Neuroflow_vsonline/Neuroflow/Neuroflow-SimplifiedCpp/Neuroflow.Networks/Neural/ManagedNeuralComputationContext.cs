using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    internal sealed class ManagedNeuralComputationContext : NeuralComputationContext
    {
        internal ManagedNeuralComputationContext(int size)
        {
            Contract.Requires(size >= 0);

            Buff = new float[size];
        }

        internal float[] Buff { get; private set; }

        public bool[] PValuePropagator_PWeightBufferFlipped { get; set; }

        public bool[] PValuePropagator_PBiasBufferFlipped { get; set; }
    }
}
