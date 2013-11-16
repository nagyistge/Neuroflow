#pragma once

#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"

namespace NeuroflowN
{
    class OCLComputeGradientDescent
    {
        static OCLVectorKernelName GD_Online_Smooth;
        static OCLVectorKernelName GD_Online;
        static OCLVectorKernelName GD_Offline_Smooth;
        static OCLVectorKernelName GD_Offline;

        OCLIntCtxSPtrT ctx;
        OCLProgramSPtrT program;

    public:
        OCLComputeGradientDescent(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) : ctx(ctx)
        {
            Build(vault);
        }

        void Build(const OCLVaultSPtrT& vault);

        void UpdateWeightsOnline(
            OCLKernelToExecute& exec,
            OCLBuffer1* lastUpdates,
            OCLBuffer1* weights,
            OCLBuffer1* gradients,
            float rate,
            float momentum,
            bool smoothing);

        void UpdateWeightsOffline(
            OCLKernelToExecute& exec,
            OCLBuffer1* lastUpdates,
            OCLBuffer1* weights,
            OCLBuffer1* gradientSums,
            int iterationCount,
            float rate,
            float momentum,
            bool smoothing);
    };
}