using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    unsafe internal static class ManagedComputeGradientDescent
    {
        internal static void UpdateWeightsOnline(
            ManagedArray lastUpdates,
            ManagedArray weights,
            ManagedArray gradients,
            float rate,
            float momentum,
            bool smoothing)
        {
            Debug.Assert(lastUpdates.Size != 0 && lastUpdates.Size == weights.Size && weights.Size == gradients.Size);

            fixed (float* pLastUpdates = lastUpdates.InternalArray, pWeights = weights.InternalArray, pGradients = gradients.InternalArray)
            {
                var lastUpdatesPtr = lastUpdates.ToPtr(pLastUpdates);
                var weightsPtr = weights.ToPtr(pWeights);
                var gradientsPtr = gradients.ToPtr(pGradients);

                if (smoothing)
                {
                    float smoothV = 1.0f - momentum;
                    for (int idx = 0; idx < weights.Size; idx++)
                    {
                        float update = gradientsPtr[idx] * rate;
                        float lastUpdate = lastUpdatesPtr[idx];
                        update = (lastUpdate * momentum) + (update * smoothV);
                        weightsPtr[idx] += update;
                        lastUpdatesPtr[idx] = update;
                    }
                }
                else
                {
                    for (int idx = 0; idx < weights.Size; idx++)
                    {
                        float update = gradientsPtr[idx] * rate;
                        float lastUpdate = lastUpdatesPtr[idx];
                        update = (lastUpdate * momentum) + update;
                        weightsPtr[idx] += update;
                        lastUpdatesPtr[idx] = update;
                    }
                }
            }
        }

        internal static void UpdateWeightsOffline(
            ManagedArray lastUpdates,
            ManagedArray weights,
            ManagedArray gradientSums,
            float count,
            float rate,
            float momentum,
            bool smoothing)
        {
            Debug.Assert(lastUpdates.Size != 0 && lastUpdates.Size == weights.Size && weights.Size == gradientSums.Size);

            fixed (float* pLastUpdates = lastUpdates.InternalArray, pWeights = weights.InternalArray, pGradients = gradientSums.InternalArray)
            {
                var lastUpdatesPtr = lastUpdates.ToPtr(pLastUpdates);
                var weightsPtr = weights.ToPtr(pWeights);
                var gradientSumsPtr = gradientSums.ToPtr(pGradients);

                if (smoothing)
                {
                    float smoothV = 1.0f - momentum;
                    for (int idx = 0; idx < weights.Size; idx++)
                    {
                        float update = (gradientSumsPtr[idx] / count) * rate;
                        float lastUpdate = lastUpdatesPtr[idx];
                        update = (lastUpdate * momentum) + (update * smoothV);
                        weightsPtr[idx] += update;
                        lastUpdatesPtr[idx] = update;
                    }
                }
                else
                {
                    for (int idx = 0; idx < weights.Size; idx++)
                    {
                        float update = (gradientSumsPtr[idx] / count) * rate;
                        float lastUpdate = lastUpdatesPtr[idx];
                        update = (lastUpdate * momentum) + update;
                        weightsPtr[idx] += update;
                        lastUpdatesPtr[idx] = update;
                    }
                }
            }
        }
    }
}
