#pragma once

#include "OCLKernelBase.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"
#include "OCLError.h"

namespace NeuroflowN
{
    class OCLComputeGradientsKernel : public OCLKernelBase
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
            std::shared_ptr<std::string> name;
            bool calcGradients;
            bool calcGradientSums;
            bool ff;
            bool bpttp1;
            bool bpttp2;
            bool bptt;
        };

        static OCLVectorKernelName ComputeGradients_FF_Online_CPU;
        static OCLVectorKernelName ComputeGradients_FF_Online_GPU;

        static OCLVectorKernelName ComputeGradients_FF_Offline_CPU;
        static OCLVectorKernelName ComputeGradients_FF_Offline_GPU;

        static OCLVectorKernelName ComputeGradients_FF_OnlineOffline_CPU;
        static OCLVectorKernelName ComputeGradients_FF_OnlineOffline_GPU;

        static OCLVectorKernelName ComputeGradients_BPTTPhase1_CPU;
        static OCLVectorKernelName ComputeGradients_BPTTPhase1_GPU;

        static OCLVectorKernelName ComputeGradients_BPTTPhase2_CPU;
        static OCLVectorKernelName ComputeGradients_BPTTPhase2_GPU;

        static OCLVectorKernelName ComputeGradients_BPTTPhase2_Offline_CPU;
        static OCLVectorKernelName ComputeGradients_BPTTPhase2_Offline_GPU;

        static std::string CreateKernelCode(GradientComputationFlags flags);

        static std::string CreateCPUKernelCode(GradientComputationFlags flags);

        static std::string CreateGPUKernelCode(GradientComputationFlags flags);

        static const OCLVectorKernelName& GetKernelName(GradientComputationFlags flags);

        inline static void ThrowUnknownFlagsEx(GradientComputationFlags flags)
        {
            using namespace std;

            stringstream e;
            e << "Unknown GradientComputationFlags value: " << (unsigned)flags << ".";
            throw_logic_error(e.str());
        }

        inline static void FillKernelPars(KernelPars& pars, GradientComputationFlags flags, bool fillName);

        inline static std::string CreateKernelHeader(const KernelPars& pars);

        void Exec(GradientComputationFlags flags, NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable, unsigned intItCount);

    public:
        OCLComputeGradientsKernel(const OCLIntCtxSPtrT& ctx) :
            OCLKernelBase(ctx)
        {
        };

        static void Build(OCLProgramBuilder& program);

        void ExecFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable);
        
        void ExecBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors, bool isInputStable);
        
        void ExecBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable, unsigned intItCount);
    };
}

