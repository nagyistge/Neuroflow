#pragma once

#include "IComputeActivation.h"
#include "OCLTypedefs.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"
#include "OCLComputeForwardKernel.h"
#include "OCLComputeInternalErrorsKernel.h"
#include "OCLComputeOutputErrorsKernel.h"
#include "OCLComputeGradientsKernel.h"

namespace NeuroflowN
{
    class OCLComputeActivation : public IComputeActivation
    {
        OCLIntCtxSPtrT ctx;

        OCLComputeForwardKernel computeForwardKernel;
        OCLComputeInternalErrorsKernel computeInternalErrorsKernel;
        OCLComputeOutputErrorsKernel computeOutputErrorsKernel;
        OCLComputeGradientsKernel computeGradientsKernel;

    public:
        OCLComputeActivation(const OCLIntCtxSPtrT& ctx) :
            ctx(ctx),
            computeForwardKernel(ctx),
            computeInternalErrorsKernel(ctx),
            computeOutputErrorsKernel(ctx),
            computeGradientsKernel(ctx)
        {
        }

        NfObject* CreateComputationState();

        static void Build(OCLProgramBuilder& program, unsigned maxConnectionCount);

        void ComputeForward(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* biases, IDeviceArray* outputs, ActivationFunction function, float alpha, bool isInputStable, bool isOutputStable);

        void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha);

        void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, IDeviceArray* desiredOutputs, ActivationFunction function, float alpha);

        void ComputeGradientsFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable);

        void ComputeGradientsBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors, bool isInputStable);

        void ComputeGradientsBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, bool isInputStable, unsigned intItCount);
    };
}
