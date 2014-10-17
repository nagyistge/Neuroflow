using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural
{
    public abstract class NeuralComputationContext : VectorComputationContext
    {
        #region Training State

        internal bool Training_IsInitialized { get; set; }

        internal bool Training_BeginOfBatch { get; set; }

        internal int Training_BPTTEllapsedForwardIterationCount { get; set; }

        internal int Training_NumberOfIterationsInBatch { get; set; }

        #endregion

        #region ErrorVectorStack State

        internal int ErrorVectorStack_Index { get; set; }

        internal bool[] ErrorVectorStack_HasVectors { get; set; }

        #endregion
    }
}
