#pragma once

#include "OCLVersionableKernelBase.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"
#include "OCLError.h"

namespace NeuroflowN
{
    class OCLComputeGradientsKernel : public OCLVersionableKernelBase
    {
        enum GradientComputationFlags
        {
            FF = 1 << 1,
            BPTTPhase1 = 1 << 2,
            BPTTPhase2 = 1 << 3,
            Online = 1 << 4,
            Offline = 1 << 5,
            CPU = 1 << 6,
            GPU = 1 << 7
        };

        struct KernelPars
        {
            OCLVectorKernelName* name;
            bool calcGradients;
            bool calcGradientSums;
            bool ff;
            bool bpttp1;
            bool bpttp2;
            bool bptt;
        };

        OCLProgramSPtrT program;

        ComputeGradientsKernelVersion GradientComputationFlagsToVersion(GradientComputationFlags flags);

        std::string CreateKernelCode(GradientComputationFlags flags);

        std::string CreateCPUKernelCode(GradientComputationFlags flags);

        std::string CreateGPUKernelCode(GradientComputationFlags flags);

        inline static void ThrowUnknownFlagsEx(GradientComputationFlags flags)
        {
            using namespace std;

            stringstream e;
            e << "Unknown GradientComputationFlags value: " << (unsigned)flags << ".";
            throw_logic_error(e.str());
        }

        inline void FillKernelPars(KernelPars& pars, GradientComputationFlags flags);

        inline std::string CreateKernelHeader(const KernelPars& pars);

        void Exec(GradientComputationFlags flags, NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount);

    public:
        OCLComputeGradientsKernel(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void Build(const OCLVaultSPtrT& vault);

        void ExecFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors);
        
        void ExecBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors);
        
        void ExecBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount);
    };
}

