#pragma once

#include "OCLKernelBase.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"

namespace NeuroflowN
{
    class OCLComputeOutputErrorsKernel : public OCLKernelBase
    {
        static OCLVectorKernelName ComputeErrors_Output_Sigmoid;
        static OCLVectorKernelName ComputeErrors_Output_Linear;

        OCLProgramSPtrT program;

    public:
        OCLComputeOutputErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
            OCLKernelBase(ctx)
        {
            Build(vault);
        }

        void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha);
    };
}