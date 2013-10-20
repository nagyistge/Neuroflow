#pragma once

#include "OCLTypedefs.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class OCLKernelBase : public NfObject
    {
    protected:
        const OCLIntCtxSPtrT ctx;

        unsigned CalculateVectorSize(DeviceArrayFVecT* inputList) const;

        std::pair<unsigned, unsigned> GetIOReduceSizesInput(DeviceArrayFVecT* inputList, const OCLBuffer1& outputs, unsigned vectorSize) const;
        std::pair<unsigned, unsigned> GetIOReduceSizesOutput(DeviceArrayVecT* inputList, const OCLBuffer1& outputs, unsigned vectorSize) const;

    public:
        OCLKernelBase(const OCLIntCtxSPtrT& ctx);
    };
}
