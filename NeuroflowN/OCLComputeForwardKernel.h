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

        std::string CreateCPUKernelCode(unsigned size);
        std::string CreateGPUKernelCode(unsigned size);

    public:
        OCLComputeForwardKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
            OCLActivationKernelBase(ctx)
        {
            Build(vault);
        };

        void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, ActivationFunction function, float alpha);
    };
}

