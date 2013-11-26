#pragma once

#include "OCLTypedefs.h"
#include "NfObject.h"
#include <boost/optional.hpp>

namespace NeuroflowN
{
    class OCLKernelBase : public NfObject
    {
    protected:
        const OCLIntCtxSPtrT ctx;

        unsigned CalculateVectorSize(DeviceArrayFVecT* inputList) const;

        std::pair<unsigned, unsigned> GetIOReduceSizesInput(DeviceArrayFVecT* inputList, OCLBuffer1* outputs, unsigned vectorSize) const;
        std::pair<unsigned, unsigned> GetIOReduceSizesOutput(DeviceArrayVecT* inputList, OCLBuffer1* outputs, unsigned vectorSize) const;

        static std::string ReplaceIndexesInTemplate(const std::string tmpl, int layerIndex)
        {
            return ReplaceIndexesInTemplate(tmpl, null, layerIndex);
        }

        static std::string ReplaceIndexesInTemplate(const std::string& tmpl, boost::optional<int> inputIndex, boost::optional<int> layerIndex);

    public:
        OCLKernelBase(const OCLIntCtxSPtrT& ctx);
    };
}
