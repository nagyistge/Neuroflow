#pragma once

#include "OCLTypedefs.h"
#include "OCLVectorKernelName.h"
#include "NNMetadata.h"
#include "OCLActivationKernelBase.h"

namespace NeuroflowN
{
    extern const char ComputeForwardTmpl [];

    class OCLComputeForwardKernel : public OCLActivationKernelBase<ComputeForwardTmpl>
    {
		OCLProgramSPtrT program;

        static std::string CreateCPUKernelCode(unsigned size);
        static std::string CreateGPUKernelCode(unsigned size);

    public:
        OCLComputeForwardKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
            OCLActivationKernelBase(ctx)
        {
        };

		void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, ActivationFunction function, float alpha);
    };
}

