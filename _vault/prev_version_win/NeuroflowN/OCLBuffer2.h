#pragma once

#include "OCLBuffer1.h"
#include "IDeviceArray2.h"

namespace NeuroflowN
{
    class OCLBuffer2 : public IDeviceArray2
    {
        OCLBuffer1 baseBuffer;
        unsigned size1;

    public:
        OCLBuffer2() = delete;
        OCLBuffer2(const cl::Buffer& buffer, unsigned size1) :
            baseBuffer(buffer),
            size1(size1)
        {
        }
        OCLBuffer2(OCLDeviceArrayPool* pool, unsigned beginIndex, unsigned size1, unsigned size2) :
            baseBuffer(pool, beginIndex, size1 * size2),
            size1(size1)
        {
        }

        DeviceArrayType GetType() const
        {
            return DeviceArrayType::DeviceArray2;
        }

        unsigned GetSize() const
        {
            return baseBuffer.GetSize();
        }

        unsigned GetSize1() const
        {
            return size1;
        }

        unsigned GetSize2() const
        {
            return GetSize() / GetSize1();
        }

        const cl::Buffer& GetCLBuffer()
        {
            return baseBuffer.GetCLBuffer();
        }

        OCLBuffer1* GetBaseBuffer() 
        {
            return &baseBuffer;
        }

        void Dump(const OCLIntCtxSPtrT& ctx, std::string title, bool toDebug) const
        {
            baseBuffer.Dump(ctx, title, toDebug);
        }
    };

    template <>
    struct GetSize<OCLBuffer2*>
    {
        inline static unsigned Get(OCLBuffer2* buff)
        {
            return buff->GetSize();
        }
    };
}