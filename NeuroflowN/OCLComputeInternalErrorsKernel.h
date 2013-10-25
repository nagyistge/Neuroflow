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

        static std::string CreateCPUKernelCode(unsigned size);
        static std::string CreateGPUKernelCode(unsigned size);

    public:
		OCLComputeInternalErrorsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
            OCLActivationKernelBase(ctx)
        {
			Build(vault);
        };

		void Build(const OCLVaultSPtrT& vault);
        void Exec(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha);
    };
}

