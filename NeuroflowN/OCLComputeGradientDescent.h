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
	public:
		static void Build(OCLProgramBuilder& program);

        static void UpdateWeightsOnline(
            OCLKernelToExecute& exec,
            const OCLIntCtxSPtrT& ctx,
            const OCLBuffer1& lastUpdates,
            const OCLBuffer1& weights,
            const OCLBuffer1& gradients,
            float rate,
            float momentum,
            bool smoothing);

        static void UpdateWeightsOffline(
            OCLKernelToExecute& exec,
            const OCLIntCtxSPtrT& ctx,
            const OCLBuffer1& lastUpdates,
            const OCLBuffer1& weights,
            const OCLBuffer1& gradientSums,
            int iterationCount,
            float rate,
            float momentum,
            bool smoothing);
    };
}