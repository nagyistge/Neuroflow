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

    public:
        OCLComputeOutputErrorsKernel(const OCLIntCtxSPtrT& ctx) :
            OCLKernelBase(ctx)
        {
        }

        static void Build(OCLProgramBuilder& program);

        void Exec(NfObject* state, IDeviceArray* pOutputs, IDeviceArray* pErrors, IDeviceArray* pDesiredOutputs, ActivationFunction function, float alpha);
    };
}