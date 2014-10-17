#pragma once

#include "IVectorUtils.h"
#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include "OCLVectorKernelName.h"

namespace NeuroflowN
{
	class OCLVectorUtils : public IVectorUtils
	{
		static OCLVectorKernelName AddMSEName;
		static OCLVectorKernelName DivName;
        static OCLVectorKernelName ZeroFName;

        OCLIntCtxSPtrT ctx;

        std::mt19937 generator;

        OCLKernelToExecute addExec, divExec, zeroFExec;

    public:
        OCLVectorUtils(const OCLIntCtxSPtrT& ctx) :
            ctx(ctx),
            generator((std::random_device()() << 16) | std::random_device()())
        {
        }

		static void Build(OCLProgramBuilder& program);

        void CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex);

        void RandomizeUniform(IDeviceArray* values, float min, float max);

        void Zero(IDeviceArray* deviceArray);

    private:
        void AddMSE(const OCLBuffer1& desiredValues, const OCLBuffer1& currentValues, const OCLBuffer1& mseValues, unsigned mseValueIndex);

        void Div(const OCLBuffer1& values, unsigned valueIndex, float byValue);
	};
}