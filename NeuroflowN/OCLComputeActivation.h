#pragma once

#include "IComputeActivation.h"
#include "OCLTypedefs.h"
#include "NNMetadata.h"
#include "OCLVectorKernelName.h"
#include "OCLComputeForwardKernel.h"
#include "OCLComputeInternalErrorsKernel.h"
#include "OCLComputeOutputErrorsKernel.h"
#include "OCLComputeGradientsKernel.h"
#include "OCLComputeGradientsRTLRKernel.h"

namespace NeuroflowN
{
    class OCLComputeActivation : public IComputeActivation
    {
        OCLIntCtxSPtrT ctx;

        OCLComputeForwardKernel computeForwardKernel;
        OCLComputeInternalErrorsKernel computeInternalErrorsKernel;
        OCLComputeOutputErrorsKernel computeOutputErrorsKernel;
        OCLComputeGradientsKernel computeGradientsKernel;
        OCLComputeGradientsRTLRKernel computeGradientsRTLRKernel;

    public:
        OCLComputeActivation(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const std::shared_ptr<OCLDeviceArrayManagement>& deviceArrayManagement) :
            ctx(ctx),
            computeForwardKernel(ctx, vault),
            computeInternalErrorsKernel(ctx, vault),
            computeOutputErrorsKernel(ctx, vault),
            computeGradientsKernel(ctx, vault),
            computeGradientsRTLRKernel(ctx, vault, deviceArrayManagement)
        {
        }

        NfObject* CreateComputationState();

        void ComputeForward(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* biases, IDeviceArray* outputs, ActivationFunction function, float alpha);

        void ComputeForwardRTLR(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* weights, IDeviceArray* biases, IDeviceArray* outputs, IDeviceArray* netValueDerivates, ActivationFunction function, float alpha);

        void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, DeviceArray2VecT* lowerWeights, DeviceArrayVecT* lowerErrors, ActivationFunction function, float alpha);

        void ComputeErrors(NfObject* state, IDeviceArray* outputs, IDeviceArray* errors, IDeviceArray* desiredOutputs, ActivationFunction function, float alpha);

        void ComputeGradientsFF(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors);

        void ComputeGradientsBPTTPhase1(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, IDeviceArray* errors);

        void ComputeGradientsBPTTPhase2(NfObject* state, DeviceArrayFVecT* inputs, DeviceArray2VecT* gradients, IDeviceArray* biasGradients, DeviceArray2VecT* gradientSums, IDeviceArray* biasGradientSums, IDeviceArray* errors, unsigned intItCount);

        void ComputeGradientsRTLR(NfObject* state, RTLRLayerInfoVecVecT* inputLayerInfos, DeviceArrayVecT* netValueDerivates, RTLRComputationData* data, DeviceArrayVecT* valueRelatedPBuffs, IDeviceArray* outputs, IDeviceArray* desiredOutputs);
    };
}
