#pragma once

#pragma once

#include "OCLTypedefs.h"
#include "OCLVectorKernelName.h"
#include "NNMetadata.h"
#include "OCLActivationKernelBase.h"

namespace NeuroflowN
{
    extern const char ComputeInternalErrorsTmpl [];

    class OCLComputeInternalErrorsKernel : public OCLActivationKernelBase<ComputeInternalErrorsTmpl>
    {
        OCLProgramSPtrT program;

        std::string CreateCPUKernelCode(unsigned size);
        std::string CreateGPUKernelCode(unsigned size);

    public:
        OCLComputeInternalErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha);
    };
}

