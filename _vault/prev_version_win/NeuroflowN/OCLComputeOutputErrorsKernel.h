#pragma once

#include "OCLKernelBase.h"
#include "NNMetadata.h"
#include "OCLVersionableKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeOutputErrorsKernel : public OCLVersionableKernelBase
    {
        OCLProgramSPtrT program;

    public:
        OCLComputeOutputErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha);
    };
}