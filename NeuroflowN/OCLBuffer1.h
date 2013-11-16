#pragma once

#include "OCLTypedefs.h"
#include "OCL.h"
#include <string>
#include "GetSize.h"
#include "IDeviceArray.h"

namespace NeuroflowN
{
    class OCLBuffer1 : public IDeviceArray
    {        
        unsigned beginIndex = 0;
        unsigned size;
        cl::Buffer buffer;
        OCLDeviceArrayPool* pool = null;

    public:
        OCLBuffer1() = delete;
        OCLBuffer1(const cl::Buffer& buffer) :
            buffer(buffer),
            size(buffer.getInfo<CL_MEM_SIZE>() / sizeof(float))
        {
        }
        OCLBuffer1(OCLDeviceArrayPool* pool, unsigned beginIndex, unsigned size) :
            beginIndex(beginIndex),
            size(size),
            pool(pool)
        {
        }

        DeviceArrayType GetType() const
        {
            return DeviceArrayType::DeviceArray;
        }

        unsigned GetSize() const
        {
            return size;
        }

        unsigned GetBeginIndex() const
        {
            return beginIndex;
        }

        cl::Buffer& GetCLBuffer();

        void Dump(const OCLIntCtxSPtrT& ctx, std::string title, bool toDebug) const;
    };

    template <>
    struct GetSize<OCLBuffer1*>
    {
        inline static unsigned Get(OCLBuffer1* buff)
        {
            return buff->GetSize();
        }
    };
}