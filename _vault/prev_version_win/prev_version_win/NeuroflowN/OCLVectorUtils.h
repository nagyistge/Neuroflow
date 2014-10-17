#pragma once

#include "IVectorUtils.h"
#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include "OCLVectorKernelName.h"
#include "OCL.h"

namespace NeuroflowN
{
    class OCLVectorUtils : public IVectorUtils
    {
        friend class OCLContextImpl;
        friend class OCLDeviceArrayPool;

        static OCLVectorKernelName AddMSEName;
        static OCLVectorKernelName DivName;
        static OCLVectorKernelName ZeroFName;

        OCLIntCtxSPtrT ctx;
        std::mt19937 generator;
        OCLProgramSPtrT program;
        OCLKernelToExecute addExec, divExec, zeroFExec;
        cl_float2 z2;
        cl_float4 z4;
        cl_float8 z8;
        cl_float16 z16;

        void Build(const OCLVaultSPtrT& vault);
        void Zero(const cl::Buffer buffer, unsigned size);
    public:
        OCLVectorUtils(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault);

        void CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex);
        void RandomizeUniform(IDeviceArray* values, float min, float max);
        void Zero(IDeviceArray* deviceArray);

    private:
        unsigned GetPreferredWorkgroupSizeMul();
        void AddMSE(OCLBuffer1* desiredValues, OCLBuffer1* currentValues, OCLBuffer1* mseValues, unsigned mseValueIndex);
        void Div(OCLBuffer1* values, unsigned valueIndex, float byValue);
    };
}