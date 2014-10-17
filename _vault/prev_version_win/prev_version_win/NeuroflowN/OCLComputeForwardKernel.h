#pragma once

#include "OCLTypedefs.h"
#include "OCLVectorKernelName.h"
#include "NNMetadata.h"
#include "OCLVersionableKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeForwardKernel : public OCLVersionableKernelBase
    {
        OCLProgramSPtrT program;

        std::string CreateCPUKernelCode(unsigned size);
        std::string CreateGPUKernelCode(unsigned size);
        void Build(const OCLVaultSPtrT& vault);

    public:
        OCLComputeForwardKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);
        
        void Exec(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* pBiases, IDeviceArray* pOutputs, IDeviceArray* pNetValueDerivates, ActivationFunction function, float alpha);
    };
}

