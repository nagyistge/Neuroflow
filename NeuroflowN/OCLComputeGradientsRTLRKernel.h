#pragma once
#include "OCLTypedefs.h"
#include "OCLVersionableKernelBase.h"

namespace NeuroflowN
{
    class OCLComputeGradientsRTLRKernel : public OCLVersionableKernelBase
    {
        OCLProgramSPtrT program;

        std::string GetKernelHeader(const char* name);
        std::string CreateCode_ComputeGradinetsRTLR_Layer_CPU();
        std::string CreateCallCode_ComputeGradinetsRTLR_Layer_CPU(unsigned layerIndex);
        std::string CreateCPUKernelCode();
        std::string CreateGPUKernelCode();
        void Build(const OCLVaultSPtrT& vault);
        void AnalyzeInfos(const RTLRLayerInfoVecVecT& infos, unsigned& vectorSize, unsigned& maxLayerSize) const;

    public:
        OCLComputeGradientsRTLRKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Exec(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs);
    };
}

