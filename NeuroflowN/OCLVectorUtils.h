#pragma once

#include "IVectorUtils.h"
#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include "OCLVectorKernelName.h"

namespace NeuroflowN
{
    class OCLVectorUtils : public IVectorUtils
    {
        friend class OCLContextImpl;

        static OCLVectorKernelName AddMSEName;
        static OCLVectorKernelName DivName;
        static OCLVectorKernelName ZeroFName;

        OCLIntCtxSPtrT ctx;
        std::mt19937 generator = std::mt19937((std::random_device()() << 16) | std::random_device()());
        OCLProgramSPtrT program;
        OCLKernelToExecute addExec = OCLKernelToExecute(true);
        OCLKernelToExecute divExec = OCLKernelToExecute(true);
        OCLKernelToExecute zeroFExec;

    public:
        OCLVectorUtils(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
            ctx(ctx)
        {
            Build(vault);
        }

        void Build(const OCLVaultSPtrT& vault);

        void CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex);

        void RandomizeUniform(IDeviceArray* values, float min, float max);

        void Zero(IDeviceArray* deviceArray);

    private:
        unsigned GetPreferredWorkgroupSizeMul();

        void AddMSE(const OCLBuffer1& desiredValues, const OCLBuffer1& currentValues, const OCLBuffer1& mseValues, unsigned mseValueIndex);

        void Div(const OCLBuffer1& values, unsigned valueIndex, float byValue);
    };
}