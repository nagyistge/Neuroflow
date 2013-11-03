#pragma once
#include "OCLTypedefs.h"
#include "OCLVersionableKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeGradientsRTLRKernel : public OCLVersionableKernelBase
    {
        OCLProgramSPtrT program;
        std::shared_ptr<OCLDeviceArrayManagement> deviceArrayManagement;
        cl::Buffer tmpGradients;

        std::string CreateCPUKernelCode(unsigned size);
        std::string CreateGPUKernelCode(unsigned size);
        void Build(const OCLVaultSPtrT& vault);
        void AnalyzeInfos(const RTLRLayerInfoVecT& infos, unsigned& vectorSize, unsigned& uCount) const;

    public:
        OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const std::shared_ptr<OCLDeviceArrayManagement>& deviceArrayManagement);

        void Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs);
    };
}

