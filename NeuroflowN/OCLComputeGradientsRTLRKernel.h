#pragma once
#include "OCLTypedefs.h"
#include "OCLVersionableKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeGradientsRTLRKernel : public OCLVersionableKernelBase
    {
        OCLProgramSPtrT program;

        std::string CreateCPUKernelCode(unsigned size);
        std::string CreateGPUKernelCode(unsigned size);
        void Build(const OCLVaultSPtrT& vault);

    public:
        OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs);
    };
}

